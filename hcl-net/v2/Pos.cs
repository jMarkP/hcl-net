using System;
using System.Diagnostics;

namespace hcl_net.v2
{
    internal struct Pos
    {
        public Pos(string filename, int @byte, int line, int column)
        {
            Filename = filename;
            Byte = @byte;
            Line = line;
            Column = column;
        }

        public string Filename { get; }

        public int Byte { get; }

        public int Line { get; }

        public int Column { get; }

        public static Pos CreateForFile(string filename)
        {
            return new Pos(filename, 0, 1, 0);
        }

        public Pos AdvancedToOffsetIn(Span<byte> bytes, int offset)
        {
            Debug.Assert(offset <= bytes.Length);
            int col = Column;
            int line = Line;
            int o = Byte;
            while (o < offset)
            {
                if (bytes[o] == '\n' || (bytes[o] == '\r' && o < bytes.Length - 1 && bytes[o + 1] == '\n'))
                {
                    col = 1;
                    line++;
                }
                else
                {
                    col++;
                }

                o++;
            }

            return new Pos(Filename, o, line, col);
        }
        public Pos NextInString(string src, out char c)
        {
            // Implement Next() as Peek() + Advance
            c = PeekInString(src);
            return new Pos(Filename, Byte + 1, 
                c == '\n' ? Line + 1 : Line,
                c == '\n' ? 0 : Column + 1);
        }

        public char PeekInString(string src)
        {
            var eof = Byte >= src.Length;
            return eof ? '\0' : src[Byte];
        }

        public bool IsValid()
        {
            return Line > 0;
        }

        public bool Before(Pos u)
        {
            return u.Byte > this.Byte
                   || u.Line > this.Line;
        }
        public bool After(Pos u)
        {
            return u.Byte < this.Byte
                   || u.Line < this.Line;
        }

        public override string ToString()
        {
            var s = Filename;
            if (IsValid())
            {
                s += (string.IsNullOrEmpty(s) 
                    ? "" : ":")
                  + Line + ":" + Column;
            }
            return string.IsNullOrEmpty(s) ? "-" : s;
        }

        public bool Equals(Pos other)
        {
            return string.Equals(Filename, other.Filename) 
                && Byte == other.Byte 
                && Line == other.Line 
                && Column == other.Column;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Pos && Equals((Pos) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Filename != null ? Filename.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ Byte;
                hashCode = (hashCode*397) ^ Line;
                hashCode = (hashCode*397) ^ Column;
                return hashCode;
            }
        }
    }
}