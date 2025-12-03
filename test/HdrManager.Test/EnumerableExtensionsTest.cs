using NUnit.Framework;
using System.Collections.Generic;

namespace HdrManager.Test
{
    [TestFixture]
    public class EnumerableExtensionsTest
    {
        [Test]
        public void EmptyIfNull_ReturnsEmptyEnumerable_WhenInputIsNull()
        {
            IEnumerable<int> input = null;

            var result = input.EmptyIfNull();

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void EmptyIfNull_ReturnsSameEnumerable_WhenInputIsNotEmpty()
        {
            IEnumerable<int> input = new List<int>() { 1, 2, 3 };

            var result = input.EmptyIfNull();

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.SameAs(input));
        }

        [Test]
        public void EmptyIfNull_ReturnsSameEnumerable_WhenInputIsEmpty()
        {
            IEnumerable<int> input = new List<int>();

            var result = input.EmptyIfNull();

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.SameAs(input));
        }
    }
}
