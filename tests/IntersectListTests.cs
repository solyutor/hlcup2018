using System.Linq;
using aspnetapp.Collections;
using FluentAssertions;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class IntersectListTests
    {
        [Test]
        public void Should_return_unique_values()
        {
            var first = new HList<uint> {18, 11, 3};
            var second = new HList<uint> {25, 18, 15, 11, 4};

            var result = new IntersectList<uint>(first, second);

            var values = result.ToArray();

            values.Should().BeEquivalentTo(new uint[] {18, 11});
        }


        [Test]
        public void Should_return_unique_values2()
        {
            var first = new HList<uint> {};
            var second = new HList<uint> {25, 18, 15, 11, 4};

            var result = new IntersectList<uint>(first, second);

            var values = result.ToArray();

            values.Should().BeEquivalentTo(new uint[0] );
        }

        [Test]
        public void Should_return_unique_values3()
        {
            var first = new HList<uint> {31, 29};
            var second = new HList<uint> {25, 18, 15, 11, 4};

            var result = new IntersectList<uint>(first, second);

            var values = result.ToArray();

            values.Should().BeEquivalentTo(new uint[0]);
        }

        [Test]
        public void Should_return_unique_many_values()
        {
            var first = new HList<uint> {18, 11, 3};
            var second = new HList<uint> {25, 18, 15, 11, 4};
            var third = new HList<uint> {11 };

            var result = new IntersectManyList<uint>(new[] {first, second, third});

            var values = result.ToArray();

            values.Should().BeEquivalentTo(new uint[] { 11});
        }

        [Test]
        public void Should_return_unique_many_values2()
        {
            var first = new HList<uint> {18, 11, 3};
            var second = new HList<uint> {25, 18, 15, 11, 4};
            var third = new HList<uint> {11 };

            var result = new UintIntersectList().Add(first).Add(second).Add(third);

            var values = result.ToArray();

            values.Should().BeEquivalentTo(new uint[] { 11});
        }
    }
}