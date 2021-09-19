namespace hcl_net.Parse.HCL
{
    internal struct Pos
    {
        private readonly string _filename;
        private readonly int _offset;
        private readonly int _line;
        private readonly int _column;

        public Pos(string filename, int offset, int line, int column)
        {
            _filename = filename;
            _offset = offset;
            _line = line;
            _column = column;
        }

        public string Filename { get { return _filename; } }

        public int Offset { get { return _offset; } }

        public int Line { get { return _line; } }

        public int Column { get { return _column; } }

        public static Pos CreateForFile(string filename)
        {
            return new Pos(filename, 0, 1, 0);
        }
        public Pos NextInString(string src, out char c)
        {
            // Implement Next() as Peek() + Advance
            c = PeekInString(src);
            return new Pos(Filename, Offset + 1, 
                c == '\n' ? Line + 1 : Line,
                c == '\n' ? 0 : Column + 1);
        }

        public char PeekInString(string src)
        {
            var eof = Offset >= src.Length;
            return eof ? '\0' : src[Offset];
        }

        public bool IsValid()
        {
            return _line > 0;
        }

        public bool Before(Pos u)
        {
            return u.Offset > this.Offset
                   || u.Line > this.Line;
        }
        public bool After(Pos u)
        {
            return u.Offset < this.Offset
                   || u.Line < this.Line;
        }

        public override string ToString()
        {
            var s = _filename;
            if (IsValid())
            {
                s += (string.IsNullOrEmpty(s) 
                    ? "" : ":")
                  + _line + ":" + _column;
            }
            return string.IsNullOrEmpty(s) ? "-" : s;
        }

        public bool Equals(Pos other)
        {
            return string.Equals(_filename, other._filename) 
                && _offset == other._offset 
                && _line == other._line 
                && _column == other._column;
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
                var hashCode = (_filename != null ? _filename.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ _offset;
                hashCode = (hashCode*397) ^ _line;
                hashCode = (hashCode*397) ^ _column;
                return hashCode;
            }
        }
    }
}