using System;

namespace hcl_net.v2
{
    internal readonly struct Range
    {
        public Range(string filename, Pos start, Pos end)
        {
            Filename = filename;
            Start = start;
            End = end;
            Length = end.Byte - start.Byte;
        }

        public string Filename { get; }
        public Pos Start { get; }
        public Pos End { get; }
        public int Length { get; }
        
        public static Range Between(Range start, Range end)
        {
            return new Range(
                filename: start.Filename,
                start: start.Start,
                end: end.End);
        }

        public override string ToString()
        {
            return Start.Line == End.Line
                ? $"{Filename}:{Start.Line},{Start.Column}-{End.Column}"
                : $"{Filename}:{Start.Line},{Start.Column}-{End.Line},{End.Column}";
        }
    }
}