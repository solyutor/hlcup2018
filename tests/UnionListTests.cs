using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using aspnetapp.Collections;
using FluentAssertions;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class UnionListTests
    {
        [Test]
        public void Should_return_unique_values_2()
        {
            var first = new HList<uint> {18, 11, 3};
            var second = new HList<uint> {25, 18, 15, 11, 4};

            var result = new MultiUnionList<uint>(new []{first, second});

            var values = result.ToArray();

            values.Should().BeEquivalentTo(new uint[] {25, 18, 15, 11, 4, 3});
        }

        [Test]
        public void Should_return_unique_values()
        {
            var first = new HList<uint> {18, 11, 3};
            var second = new HList<uint> {25, 18, 15, 11, 4};

            var result = new UnionList<uint>(first, second);

            var values = result.ToArray();

            values.Should().BeEquivalentTo(new uint[] {25, 18, 15, 11, 4, 3});
        }


        [Test]
        public void Should_return_unique_values2()
        {
            var first = new HList<uint> {};
            var second = new HList<uint> {25, 18, 15, 11, 4};

            var result = new UnionList<uint>(first, second);

            var values = result.ToArray();

            values.Should().BeEquivalentTo(new uint[] {25, 18, 15, 11, 4});
        }

        [Test]
        public void Should_return_unique_values3()
        {
            var first = new HList<uint> {31, 29};
            var second = new HList<uint> {25, 18, 15, 11, 4};

            var result = new UnionList<uint>(first, second);

            var values = result.ToArray();

            values.Should().BeEquivalentTo(new uint[] {31, 29, 25, 18, 15, 11, 4});
        }

        [Test]
        public void Should_return_unique_values4()
        {
            var first = new HList<uint> {31, 29};
            var second = new HList<uint> {25, 18, 15, 11, 4};
            var third = new HList<uint> {15, 11, 4, 3};

            var result = new UnionList<uint>(third, new UnionList<uint>(first, second));

            var values = result.ToArray();
            Console.WriteLine(string.Join(',', values));
            values.Should().BeEquivalentTo(new uint[] {31, 29, 25, 18, 15, 11, 4, 3});
        }


        [Test]
        public void Should_work_properly_with_multiple_lists()
        {
            var first = new HList<uint> {29905,28722,27102,26286,25340,25043,24896,24220,23764,23530,22636,22611,21644,20639,20311,17854,17698,16742,16500,15971,15946,15890,15702,15060,13737,13313,13011,10628,9138,9000,8289,8082,7127,6791,6480,5919,5014,4805,3759,3025,2894,1459,959,667,194};
            var second = new HList<uint> {30000,29927,29886,29576,29566,29469,29155,29027,28774,28700,28698,28528,28387,28335,28301,28233,28162,28100,28088,27750,27701,27539,27384,27342,27333,26992,26843,26706,26670,26639,26344,26127,25827,25696,25642,25318,25068,25050,24983,24979,24755,24521,24296,24127,24044,23949,23869,23838,23574,23502,23442,23274,23098,22854,22642,22624,22203,22139,21865,21573,21488,21334,21315,21140,21064,21045,21022,20999,20989,20951,20936,20917,20591,20425,20397,20332,20203,20200,20170,20046,19935,19779,19605,19322,19313,19285,19258,19082,18928,18778,18682,18571,18500,18433,18242,17970,17863,17803,17604,17495,16843,16723,16663,16645,16611,16343,16317,16159,15745,15580,15485,15186,15172,14915,14859,14854,14642,14558,14324,14235,14005,13975,13877,13865,13837,13782,13780,13705,13618,13569,13379,13339,13311,13269,13189,13086,13083,12704,12591,12522,12521,12499,12456,12323,12287,12152,11899,11457,11320,11194,11144,10798,10731,10519,10475,10408,10384,10383,10319,10130,10104,10066,9763,9735,9330,8604,8180,7892,7750,7565,7537,7518,7516,7356,7312,7239,6884,6434,6219,6144,5992,5881,5764,5482,5310,5135,5132,4728,4683,4241,4218,4136,3907,3880,3798,3689,3667,3550,3496,3486,3131,3117,2794,2630,2483,2353,2331,2262,2020,1348,1283,1169,1022,858,735,694,444,61};
            var third = new HList<uint> {29514,29374,29025,28165,27911,27369,27330,27264,27075,26994,26940,26845,26541,26075,25698,25658,25224,25183,25018,25014,24832,24767,24760,24413,24410,24382,24348,24285,24278,24265,24070,24045,23996,23983,23605,23267,23230,23212,23077,22800,22714,22712,22666,22549,22545,22154,21982,21964,21946,21679,21601,21588,21532,21500,21497,21353,21218,21176,21170,21167,20925,20670,20269,19657,19240,18829,18184,18076,18064,18025,17876,17453,17337,17169,16807,16446,16211,16015,15685,15549,15524,15142,13543,13486,13045,12770,12306,12276,11998,11812,11106,11038,10938,10933,10883,10170,9829,9658,9584,9293,8482,8275,8112,8023,7916,7666,7207,5710,5632,5473,5446,4778,4761,4742,4602,4372,3701,2910,2749,2671,2270,2222,1946,1740,1568,1495,1125,918,718,370,195,92,28};
            var forth = new HList<uint> {29644,29560,29346,28810,28791,28735,28655,28627,28624,28609,28375,27858,27680,27522,27424,27295,27128,27068,26681,26534,26496,26137,26069,26053,26012,25843,25838,25829,25800,25736,25700,25670,25561,25361,25228,24935,24694,24597,24554,24466,24105,24066,24020,23835,23830,23714,23383,23309,23207,22984,22977,22584,22496,22459,22413,22303,21758,21569,21562,21482,21472,21351,21282,21270,21259,21214,21127,21032,20739,19774,19458,19281,18792,18364,18191,18178,18096,18083,17967,17685,17680,17480,17445,17257,17134,17085,16907,16709,16654,16604,16588,16557,16466,16459,16362,16153,16040,16037,15913,15302,15208,15073,14860,14539,14135,14116,14064,14001,13947,13907,13804,13775,13723,13663,13275,12891,12670,12649,12008,11935,11891,11869,11705,11643,11494,11440,11415,11371,11195,11148,11049,11026,10870,10846,10797,10736,10658,10561,10273,10138,10039,9959,9898,9515,9308,9158,9094,9028,8904,8842,8242,7695,6970,6968,6867,6826,6720,6703,6591,6519,6422,6354,6289,6241,5947,5866,5549,5386,5251,5079,5069,5047,4810,4599,4567,4450,4178,4097,4080,3782,3755,3431,3399,3104,2976,2810,2766,2571,2343,2259,2125,2022,1990,1980,1936,1917,1308,1295,1244,1089,878,620,522,392,90,89};

            var result = new UnionList<uint>(forth, new UnionList<uint>(third, new UnionList<uint>(first, second)));

            var values = result.ToArray();

            var grouped = values.GroupBy(x => x)
                .Select(group => new
                {
                    Metric = group.Key,
                    Count = group.Count()
                })
                .OrderByDescending(x => x.Count);

            foreach (var group in grouped)
            {
                Console.WriteLine($"{group.Metric}: {group.Count}");
            }
        }


        [Test]
        public void METHOD()
        {
            var iterations = 100_000_000;
            var list = new List<uint> {1, 2, 3, 4, 5};
            var hlist = new HList<uint> {1, 2, 3, 4, 5};

            var watch1 = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                foreach (var u in list)
                {

                }
            }
            watch1.Stop();

            var watch2 = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                foreach (var u in hlist)
                {

                }
            }
            watch2.Stop();

            Console.WriteLine($"List {watch1.ElapsedMilliseconds}; HList {watch2.ElapsedMilliseconds}");
        }

    }
}