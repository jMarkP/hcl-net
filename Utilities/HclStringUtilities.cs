using System;
using System.Linq;
using System.Text;

namespace hcl_net.Utilities
{
    public static class HclStringUtilities
    {
        public static string UnquoteHclString(this string s, out string error)
        {
            var n = s.Length;
            if (n < 2)
            {
                error = "input must be at least 2 characters";
                return "";
            }
            var quote = s[0];
            if (quote != '"' || s[n - 1] != quote)
            {
                error = "input must start and end with \"";
                return "";
            }
            s = s.Substring(1, n - 2);

            // Is it trivial?
            if (!s.Contains('\\')
                && !s.Contains(quote)
                && !s.Contains('$'))
            {
                error = null;
                return s;
            }

            var sb = new StringBuilder(s.Length * 3 / 2);
            var i = 0;
            while (i < s.Length)
            {
                // If we're starting a '${}' then let it through un-unquoted.
                // Specifically: we don't unquote any characters within the `${}`
                // section, except for escaped backslashes, which we handle specifically.
                if (s[i] == '$'
                    && i < s.Length - 1
                    && s[i + 1] == '{')
                {
                    sb.Append("${");
                    i += 2;
                    // Continue reading until we find the closing brace (or the end of the string), copying as-is
                    var braces = 1;
                    while (i < s.Length && braces > 0)
                    {
                        var r = s[i];
                        i++;

                        // We special case escaped backslashes in interpolations, converting
                        // them to their unescaped equivalents.
                        if (r == '\\')
                        {
                            var q = s[i];
                            if (q == '\\') continue;
                        }
                        sb.Append(r);
                        if (r == '{')
                            braces++;
                        if (r == '}')
                            braces--;
                    }
                    if (braces != 0)
                    {
                        // If we've got here then we've reached the end
                        // of the string with unbalanced braces
                        error = "Unmatched braces";
                        return "";
                    }
                    if (i == s.Length)
                    {
                        // We're done!
                        break;
                    }
                    continue;
                }

                var c = UnquoteChar(s, ref i, quote, out error);
                if (c == null)
                    return "";
                sb.Append(c);
                if (quote == '\'' && i != s.Length - 1)
                {
                    error = "Single quited string must be single character";
                    return "";
                }
            }
            error = null;
            return sb.ToString();
        }

        private static string UnquoteChar(string s, ref int i, char quote, out string error)
        {
            var c = s[i];
            i++;
            if (c == quote && (quote == '\'' || quote == '"'))
            {
                error = "input must not contain unescaped quote";
                return null;
            }
            if (c != '\\')
            {
                // Unescaped
                error = null;
                return c.ToString();
            }
            if (i > s.Length - 1)
            {
                error = "input ends with escape character";
                return null;
            }
            // Chomp the next character to see what we should be escaping
            c = s[i];
            i ++;
            switch (c)
            {
                // Special escape characters
                case 'a':
                    error = null;
                    return '\a'.ToString();
                case 'b':
                    error = null;
                    return '\b'.ToString();
                case 'f':
                    error = null;
                    return '\f'.ToString();
                case 'n':
                    error = null;
                    return '\n'.ToString();
                case 'r':
                    error = null;
                    return '\r'.ToString();
                case 't':
                    error = null;
                    return '\t'.ToString();
                case 'v':
                    error = null;
                    return '\v'.ToString();
                // Hexadecimal
                case 'x':
                case 'u':
                case 'U':
                    var n = 0;
                    switch (c)
                    {
                        case 'x':
                            n = 2;
                            break;
                        case 'u':
                            n = 4;
                            break;
                        case 'U':
                            n = 8;
                            break;
                    }
                    return ConvertFromNumberToChar(s, 16, ref i, out error, n);
                // Octal
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                    // Rewind one char so the Convert function below will see 'c' again
                    i--;
                    return ConvertFromNumberToChar(s, 8, ref i, out error, 3);
                // Misc
                case '\\':
                    error = null;
                    return '\\'.ToString();
                case '\'':
                case '"':
                    if (c != quote)
                    {
                        error = "Escaped wrong quote";
                        return null;
                    }
                    error = null;
                    return c.ToString();
                default:
                    error = "Unexpected escaped char " + c;
                    return null;

            }
        }

        private static string ConvertFromNumberToChar(string s, int @base, ref int i, out string error, int n)
        {
            if (i + n > s.Length)
            {
                error = "Invalid escaped character code";
                return null;
            }
            uint v;
            try
            {
                v = Convert.ToUInt32(s.Substring(i, n), @base);
                i += n;
                error = null;
                return char.ConvertFromUtf32((int)v);
            }
            catch (Exception ex)
            {
                error = "Error reading hex string: " + ex.Message;
                return null;
            }
        }
    }
}
