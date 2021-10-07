using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using cty_net;

namespace hcl_net.v2
{
    internal class Traversal : IEnumerable<ITraverser>
    {
        public string RootName
        {
            get
            {
                if (IsRelative)
                {
                    throw new Exception($"Can't use {nameof(RootName)} on a relative traversal");
                }

                return ((TraverseRoot) this[0]).Name;
            }
        }

        public Range SourceRange
        {
            get
            {
                if (Count == 0)
                {
                    return default;
                }

                return Range.Between(this[0].SourceRange, this[Count - 1].SourceRange);
            }
        }

        public bool IsRelative
        {
            get
            {
                if (Count == 0)
                {
                    return true;
                }

                if (this[0] is TraverseRoot)
                {
                    return false;
                }

                return true;
            }
        }
        
        public (Value, Diagnostics) TraverseRel(Value val)
        {
            if (!IsRelative)
            {
                throw new Exception($"Can't use {nameof(TraverseRel)} on an absolute traversal");
            }

            var current = val;
            var diags = new Diagnostics();
            foreach (var tr in this)
            {
                Diagnostics newDiags;
                (current, newDiags) = tr.TraversalStep(current);
                diags = diags.Append(newDiags);
                if (newDiags.HasErrors)
                {
                    return (Value.DynamicVal, diags);
                }
            }
            
            return (current, diags);
        }

        public (Value, Diagnostics) TraverseAbs(EvalContext ctx)
        {
            if (IsRelative)
            {
                throw new Exception($"Can't use {nameof(TraverseAbs)} on a relative traversal");
            }

            var split = SimpleSplit();
            var root = split.Abs[0] as TraverseRoot;
            var name = root!.Name;
            // it's acceptable to have "Variables" set to nil in an EvalContext, if
            // a particular scope has no variables at all. If _no_ contexts in the
            // chain have non-nil variables, this is considered to mean that variables
            // are not allowed at all, which produces a different error message.
            var anyContextHasVariables = false;
            EvalContext? currCtx = ctx;
            while (currCtx != null)
            {
                if (!currCtx.Variables.Any())
                {
                    currCtx = currCtx.Parent;
                    continue;
                }

                anyContextHasVariables = true;
                if (currCtx.Variables.TryGetValue(name, out var val))
                {
                    return split.Rel.TraverseRel(val);
                }

                currCtx = currCtx.Parent;
            }

            if (!anyContextHasVariables)
            {
                return (Value.DynamicVal, new Diagnostics(new Diagnostic(
                    severity: DiagnosticSeverity.Error,
                    summary: "Variables not allowed",
                    detail: "Variables may not be used here.",
                    subject: root.SourceRange)));
            }

            var suggestions = new List<string>();
            
            currCtx = ctx;
            while (currCtx != null)
            {
                suggestions.AddRange(currCtx.Variables.Keys);
                currCtx = ctx.Parent;
            }

            var suggestion = DidYouMeanExtensions.NameSuggestion(name, suggestions);
            if (!string.IsNullOrEmpty(suggestion))
            {
                suggestion = $" Did you mean {suggestion}?";
            }
            
            return (Value.DynamicVal, new Diagnostics(new Diagnostic(
                severity: DiagnosticSeverity.Error,
                summary: "Unknown variable",
                detail: $"There is no variable named {name}.{suggestion}",
                subject: root.SourceRange)));
        }

        public TraversalSplit SimpleSplit()
        {
            if (IsRelative)
            {
                throw new Exception("Can't use SimpleSplit on a relative traversal");
            }

            return new TraversalSplit(new Traversal(this[0]), new Traversal(this.Skip(1)));
        }
        
        #region IList implementation
        private readonly ITraverser[] _items;

        public Traversal()
        {
            _items = Array.Empty<ITraverser>();
        }

        public Traversal(ITraverser single) : this(new[] {single})
        {
        }

        public Traversal(IEnumerable<ITraverser> items)
        {
            _items = items.ToArray();
        }
        public IEnumerator<ITraverser> GetEnumerator()
        {
            return ((IEnumerable<ITraverser>)_items).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _items).GetEnumerator();
        }

        public ITraverser this[int index]
        {
            get => _items[index];
            set => _items[index] = value;
        }

        public int Count => _items.Length;

        #endregion
    }

    internal class TraverseRoot : ITraverser
    {
        public TraverseRoot(string name, Range srcRange)
        {
            Name = name;
            SourceRange = srcRange;
        }

        public string Name { get; }
        public Range SourceRange { get; }
        
        public (Value, Diagnostics) TraversalStep(Value val)
        {
            throw new NotImplementedException("Cannot traverse an absolute traversal");
        }
    }

    /// <summary>
    /// TraversalSplit represents a pair of traversals, the first of which is
    /// an absolute traversal and the second of which is relative to the first.
    ///
    /// This is used by calling applications that only populate prefixes of the
    /// traversals in the scope, with Abs representing the part coming from the
    /// scope and Rel representing the remaining steps once that part is
    /// retrieved.
    /// </summary>
    internal class TraversalSplit
    {
        public TraversalSplit(Traversal abs, Traversal rel)
        {
            Abs = abs;
            Rel = rel;
        }

        public Traversal Abs { get; }
        public Traversal Rel { get; }

        public string RootName => Abs.RootName;

        public (Value, Diagnostics) TraverseAbs(EvalContext ctx)
        {
            return Abs.TraverseAbs(ctx);
        }
        
        public (Value, Diagnostics) TraverseRel(Value val)
        {
            return Rel.TraverseRel(val);
        }
        
        public (Value, Diagnostics) Traverse(EvalContext ctx)
        {
            var (val1, diags1) = TraverseAbs(ctx);
            if (diags1.HasErrors)
            {
                return (Value.DynamicVal, diags1);
            }

            var (val2, diags2) = TraverseRel(val1);
            return (val2, diags1.Append(diags2));
        }

        public Traversal Join()
        {
            if (Abs.IsRelative)
            {
                throw new Exception($"First part of {nameof(TraversalSplit)} must be absolute");
            }

            if (!Rel.IsRelative)
            {
                throw new Exception($"Second part of {nameof(TraversalSplit)} must be relative");
            }

            return new Traversal(Abs.Concat(Rel));
        }
    }

    internal class TraverseAttr : ITraverser
    {
        public TraverseAttr(string name, Range sourceRange)
        {
            Name = name;
            SourceRange = sourceRange;
        }

        public string Name { get; }
        public (Value, Diagnostics) TraversalStep(Value val)
        {
            return OpsExtensions.GetAttr(val, Name, SourceRange);
        }

        public Range SourceRange { get; }
    }
    
    internal class TraverseIndex : ITraverser
    {
        public TraverseIndex(Value key, Range sourceRange)
        {
            Key = key;
            SourceRange = sourceRange;
        }

        public Value Key { get; }
        public (Value, Diagnostics) TraversalStep(Value val)
        {
            return OpsExtensions.Index(val, Key, SourceRange);
        }

        public Range SourceRange { get; }
    }
    
    internal class TraverseSplat : ITraverser
    {
        public TraverseSplat(string name, Range sourceRange)
        {
            Name = name;
            SourceRange = sourceRange;
        }

        public string Name { get; }
        public (Value, Diagnostics) TraversalStep(Value val)
        {
            throw new NotImplementedException("TraverseSplat not yet implemented");
        }

        public Range SourceRange { get; }
    }

    internal static class DidYouMeanExtensions
    {
        public static string NameSuggestion(string given, IEnumerable<string> suggestions)
        {
            return suggestions.FirstOrDefault(s => Fastenshtein.Levenshtein.Distance(given, s) < 3) ?? "";
        }
    }

    internal static class OpsExtensions
    {
        /// <summary>
        /// Index is a helper function that performs the same operation as the index
        /// operator in the HCL expression language. That is, the result is the
        /// same as it would be for collection[key] in a configuration expression.
        ///
        /// This is exported so that applications can perform indexing in a manner
        /// consistent with how the language does it, including handling of null and
        /// unknown values, etc.
        ///
        /// Diagnostics are produced if the given combination of values is not valid.
        /// Therefore a pointer to a source range must be provided to use in diagnostics,
        /// though nil can be provided if the calling application is going to
        /// ignore the subject of the returned diagnostics anyway.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="key"></param>
        /// <param name="srcRange"></param>
        /// <returns></returns>
        public static (Value, Diagnostics) Index(Value collection, Value key, Range srcRange)
        {
            const string invalidIndex = "Invalid index";

            if (collection.IsNull)
            {
                return (Value.DynamicVal, new Diagnostics(new Diagnostic(
                    severity: DiagnosticSeverity.Error,
                    summary: "Attempt to index a null value",
                    detail: "This value is null, so it does not have any indices.",
                    subject: srcRange)));
            }
            if (key.IsNull)
            {
                return (Value.DynamicVal, new Diagnostics(new Diagnostic(
                    severity: DiagnosticSeverity.Error,
                    summary: invalidIndex,
                    detail: "Can't use a null value as an indexing key.",
                    subject: srcRange)));
            }

            var collectionType = collection.Type;
            var keyType = key.Type;
            if (collectionType == DynamicPseudoType.Instance || keyType == DynamicPseudoType.Instance)
            {
                return (Value.DynamicVal, Diagnostics.None);
            }

            // Will implement the rest later
            throw new NotImplementedException();
        }

        /// <summary>
        /// GetAttr is a helper function that performs the same operation as the
        /// attribute access in the HCL expression language. That is, the result is the
        /// same as it would be for obj.attr in a configuration expression.
        ///
        /// This is exported so that applications can access attributes in a manner
        /// consistent with how the language does it, including handling of null and
        /// unknown values, etc.
        ///
        /// Diagnostics are produced if the given combination of values is not valid.
        /// Therefore a pointer to a source range must be provided to use in diagnostics,
        /// though nil can be provided if the calling application is going to
        /// ignore the subject of the returned diagnostics anyway.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="attrName"></param>
        /// <param name="srcRange"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static (Value, Diagnostics) GetAttr(Value obj, string attrName, Range srcRange)
        {
            if (obj.IsNull)
            {
                return (Value.DynamicVal, new Diagnostics(new Diagnostic(
                    severity: DiagnosticSeverity.Error,
                    summary: "Attempt to get attribute from null value",
                    detail: "This value is null, so it does not have any attributes.",
                    subject: srcRange)));
            }
            
            // Will implement the rest later
            throw new NotImplementedException();
        }
    }
}