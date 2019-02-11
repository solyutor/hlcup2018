using aspnetapp;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class SexStatusTests
    {
        /*public const SexStatus State = SexStatus.Male | SexStatus.Free;

        [TestCase(State, SexStatus.Male, true)] //sex_eq m
        [TestCase(State, SexStatus.Female, false)] //sex_eq m
        [TestCase(State, SexStatus.Free, true)] //status_eq m
        [TestCase(State, SexStatus.NotFree, false)] //status_eq m

        // status eq
        [TestCase(State, SexStatus.Male | SexStatus.Free, true)] //status_eq m
        [TestCase(State, SexStatus.Female | SexStatus.Free, false)] //status_eq m
        [TestCase(State, SexStatus.Male | SexStatus.NotFree, false)] //status_eq m
        [TestCase(State, SexStatus.Female | SexStatus.NotFree, false)] //status_eq m
        [TestCase(State, SexStatus.Free, true)] //status_eq m
        [TestCase(State, SexStatus.Complex, false)] //status_eq m

        //status_neq tests
        [TestCase(State, SexStatus.Free | SexStatus.Complex, true)] //status_neq 
        [TestCase(State, SexStatus.NotFree | SexStatus.Complex, false)] //status_neq
        //[TestCase(SexStatus.Female | SexStatus.NotFree, SexStatus.NotFree | SexStatus.Free, true)] //status_neq
        public void Should_return_valid_result(SexStatus value, SexStatus mask, bool expected)
            => Assert.That(value.Matches(mask), Is.EqualTo(expected));*/
    }
}