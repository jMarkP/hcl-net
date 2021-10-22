using System.Collections.Generic;

namespace hcl_net.v2
{
    internal interface IBody
    {
        /// <summary>
        /// Content verifies that the entire body content conforms to the given
        /// schema and then returns it, and/or returns diagnostics. The returned
        /// body content is valid if non-nil, regardless of whether Diagnostics
        /// are provided, but diagnostics should still be eventually shown to
        /// the user.
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        (BodyContent, Diagnostics) Content(BodySchema schema);
        /// <summary>
        /// PartialContent is like Content except that it permits the configuration
        /// to contain additional blocks or attributes not specified in the
        /// schema. If any are present, the returned Body is non-nil and contains
        /// the remaining items from the body that were not selected by the schema.
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        (BodyContent, IBody, Diagnostics) PartialContent(BodySchema schema);

        /// <summary>
        /// JustAttributes attempts to interpret all of the contents of the body
        /// as attributes, allowing for the contents to be accessed without a priori
        /// knowledge of the structure.
        ///
        /// The behavior of this method depends on the body's source language.
        /// Some languages, like JSON, can't distinguish between attributes and
        /// blocks without schema hints, but for languages that _can_ error
        /// diagnostics will be generated if any blocks are present in the body.
        ///
        /// Diagnostics may be produced for other reasons too, such as duplicate
        /// declarations of the same attribute.
        /// </summary>
        /// <returns></returns>
        (Attributes, Diagnostics) JustAttributes();

        /// <summary>
        /// MissingItemRange returns a range that represents where a missing item
        /// might hypothetically be inserted. This is used when producing
        /// diagnostics about missing required attributes or blocks. Not all bodies
        /// will have an obvious single insertion point, so the result here may
        /// be rather arbitrary.
        /// </summary>
        /// <returns></returns>
        Range MissingItemRange();
    }
}