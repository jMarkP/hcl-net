using System;
using hcl_net.Parser.HCL;
using NUnit.Framework;

namespace hcl_net.Test
{
    [TestFixture]
    public class PosTests
    {
        [Test]
        public void ExposesProperties()
        {
            var SUT = new Pos("Filename", 4, 8, 15);
            Assert.That(SUT.Filename, Is.EqualTo("Filename"));
            Assert.That(SUT.Offset, Is.EqualTo(4));
            Assert.That(SUT.Line, Is.EqualTo(8));
            Assert.That(SUT.Column, Is.EqualTo(15));
        }

        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        public void IsValidIfLineIsGreaterThan0(int line, bool isValid)
        {
            var SUT = new Pos("Filename", 10, line, 5);
            Assert.That(SUT.IsValid, Is.EqualTo(isValid));
        }

        [TestCase("Filename", 1, 2, 3, "Filename:2:3")]
        [TestCase("", 1, 2, 3, "2:3")]
        [TestCase(null, 1, 2, 3, "2:3")]
        [TestCase("Filename", 1, 0, 3, "Filename")]
        [TestCase(null, 1, 0, 3, "-")]
        public void ToStringHasExpectedFormat(string filename, int offset, int line, int column, string expected)
        {
            var SUT = new Pos(filename, offset, line, column);
            Assert.That(SUT.ToString(), Is.EqualTo(expected));
        }

        [Test]
        public void AllowsNullFilename()
        {
            var SUT = new Pos(null, 4, 8, 15);
            Assert.That(SUT.Filename, Is.Null);
            Assert.That(SUT.ToString(), Is.EqualTo("8:15"));
        }

        [TestCase(10, 1, 12, 1, true)]
        [TestCase(10, 1, 9, 1, false)]
        [TestCase(10, 1, 10, 1, false)]
        [TestCase(10, 1, 10, 2, true)]
        [TestCase(10, 2, 10, 1, false)]
        public void Before_ComparesOffsetThenLine(int offset, int line, int compareOffset, int compareLine, bool expected)
        {
            var SUT = new Pos(null, offset, line, 1);
            var compareTo = new Pos(null, compareOffset, compareLine, 1);
            Assert.That(SUT.Before(compareTo), Is.EqualTo(expected));
        }

        [TestCase(10, 1, 12, 1, false)]
        [TestCase(10, 1, 9, 1, true)]
        [TestCase(10, 1, 10, 1, false)]
        [TestCase(10, 1, 10, 2, false)]
        [TestCase(10, 2, 10, 1, true)]
        public void After_ComparesOffsetThenLine(int offset, int line, int compareOffset, int compareLine, bool expected)
        {
            var SUT = new Pos(null, offset, line, 1);
            var compareTo = new Pos(null, compareOffset, compareLine, 1);
            Assert.That(SUT.After(compareTo), Is.EqualTo(expected));
        }
    }
}
