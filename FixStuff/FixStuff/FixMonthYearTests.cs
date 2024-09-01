using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

namespace FixStuff
{
    [TestFixture]
    public class FixMonthYearTests
    {
        [Test]
        [TestCase(2024, 9, "202409")]
        [TestCase(2024, 10, "202410")]
        public void Construction_MonthAndYear(int year, int month, string expected)
        {
            var my = new FixMonthYear(year, month);
            Assert.That(my.AsString(), Is.EqualTo(expected));
            Assert.That(my.Length, Is.EqualTo(6));
            Assert.That(my.IsValid, Is.True);
        }

        [Test]
        [TestCase(2024, 9, 1, "20240901")]
        [TestCase(2024, 10, 31, "20241031")]
        public void Construction_MonthAndYearAndDayOfMonth(int year, int month, int dayOfMonth, string expected)
        {
            var my = new FixMonthYear(year, month, dayOfMonth);
            Assert.That(my.AsString(), Is.EqualTo(expected));
            Assert.That(my.Length, Is.EqualTo(8));
            Assert.That(my.IsValid, Is.True);
        }

        [Test]
        [TestCase(2024, 9, WeekCode.W1, "202409w1")]
        [TestCase(2024, 9, WeekCode.W2, "202409w2")]
        [TestCase(2024, 9, WeekCode.W3, "202409w3")]
        [TestCase(2024, 9, WeekCode.W4, "202409w4")]
        [TestCase(2024, 9, WeekCode.W5, "202409w5")]
        public void Construction_MonthAndYearAndWeekfMonth(int year, int month, WeekCode weekCode, string expected)
        {
            var my = new FixMonthYear(year, month, weekCode);
            Assert.That(my.AsString(), Is.EqualTo(expected));
            Assert.That(my.Length, Is.EqualTo(8));
            Assert.That(my.IsValid, Is.True);
        }

        [Test]
        public void Construction_Default()
        {
            var my = new FixMonthYear();
            Assert.That(my.AsString(), Is.EqualTo(""));
            Assert.That(my.Length, Is.EqualTo(0));
            Assert.That(my.IsValid, Is.False);
        }

        [Test]
        public void Construction_LeapYear()
        {
            var my = new FixMonthYear(2024, 2, 29);
            Assert.That(my.AsString(), Is.EqualTo("20240229"));
        }

        [Test]
        public void Construction_NotLeapYear()
        {
            Assert.Catch(() => new FixMonthYear(2023, 2, 29));
        }

        [Test]
        public void Construction_30DayMonths()
        {
            Assert.Catch(() => new FixMonthYear(2023, 4, 31));
            Assert.Catch(() => new FixMonthYear(2023, 6, 31));
            Assert.Catch(() => new FixMonthYear(2023, 9, 31));
            Assert.Catch(() => new FixMonthYear(2023, 11, 31));
        }
    }
}
