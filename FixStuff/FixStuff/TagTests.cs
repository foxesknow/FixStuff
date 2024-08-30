using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

namespace FixStuff
{
    [TestFixture]
    public class TagTests
    {
        [Test]
        [TestCaseSource(typeof(TagTests), nameof(TagTests.IntTestCases))]
        public void Construction_FromInt(int value)
        {
            var tag = new Tag(value);
            Assert.That(tag.IsValid, Is.True);
            Assert.That(tag.Value, Is.EqualTo(value));
        }

        [Test]
        [TestCaseSource(typeof(TagTests), nameof(TagTests.StringTestCases))]
        public void Construction_FromString(string value)
        {
            var tag = new Tag(value);
            Assert.That(tag.IsValid, Is.True);
            Assert.That(tag.Value, Is.EqualTo(int.Parse(value)));
            Assert.That(tag.Length, Is.EqualTo(value.Length));
            Assert.That(tag.AsString(), Is.EqualTo(value));
        }

        [Test]
        [TestCase(-1)]
        [TestCase("123456")]
        public void Construction_FromInvalidNumber(int value)
        {
            Assert.Catch(() => new Tag(value));
        }

        [Test]
        [TestCase("")]
        [TestCase("123456")]
        [TestCase("Hello")]
        public void Construction_FromInvalidString(string value)
        {
            Assert.Catch(() => new Tag(value));
        }

        [Test]
        [TestCaseSource(typeof(TagTests), nameof(TagTests.StringTestCases))]
        public void IndexAccess(string value)
        {
            var tag = new Tag(value);
            Assert.That(tag.IsValid, Is.True);
            
            for(int i = 0; i < value.Length; i++)
            {
                Assert.That(value[i], Is.EqualTo((char)tag[i]));
            }
        }

        [Test]
        public void IndexAccessFromEnd()
        {
            var tag = new Tag(12345);
            Assert.That(tag.IsValid, Is.True);
            
            Assert.That(tag[^1], Is.EqualTo((byte)'5'));
        }

        [Test]
        public void CopyTo()
        {
            var tag = new Tag(246);
            Span<byte> destination = stackalloc byte[3];
            
            int bytesCopied = tag.CopyTo(destination);
            Assert.That(bytesCopied, Is.EqualTo(3));
            
            Assert.That(destination[0], Is.EqualTo((byte)'2'));
            Assert.That(destination[1], Is.EqualTo((byte)'4'));
            Assert.That(destination[2], Is.EqualTo((byte)'6'));

        }

        [Test]
        public void Equality()
        {
            var none = Tag.None;
            Assert.That(Tag.None == none, Is.True);

            var tag1 = new Tag(12345);
            var tag2 = new Tag(12345);
            var tag3 = new Tag(92345);

            Assert.That(tag1 == tag2, Is.True);
            Assert.That(tag1 == tag3, Is.False);
        }

        [Test]
        public void Inequality()
        {
            var none = Tag.None;
            Assert.That(Tag.None != none, Is.False);

            var tag1 = new Tag(12345);
            var tag2 = new Tag(12345);
            var tag3 = new Tag(92345);

            Assert.That(tag1 != tag2, Is.False);
            Assert.That(tag1 != tag3, Is.True);
        }

        private static IEnumerable<TestCaseData> IntTestCases()
        {
            return TagValues().Select(i => new TestCaseData(i));
        }

        private static IEnumerable<TestCaseData> StringTestCases()
        {
            return TagValues().Select(i => new TestCaseData(i.ToString()));
        }

        private static IEnumerable<int> TagValues()
        {
            return 
            [
                0, 1, 9, 10, 99, 100, 999, 1000, 9999, 10000, 99999,
                12, 123, 1234, 12345
            ];
        }
    }
}
