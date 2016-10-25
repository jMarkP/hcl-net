using System;

namespace hcl_net.Parser.HCL
{
    internal class Pos
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
    }
}