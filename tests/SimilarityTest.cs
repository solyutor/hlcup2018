using System;
using System.Collections.Generic;
using System.Diagnostics;
using aspnetapp;
using aspnetapp.Collections;
using aspnetapp.Controllers;
using NUnit.Framework;

namespace Tests
{


    [TestFixture]
    public class SimilarityTest
    {
        private Account _me;
        private Account _candidate;

        [SetUp]
        public void Setup()
        {
            var meLikes = new List<LikeStub> {new LikeStub {id = 8279, ts = 1460786749}, new LikeStub {id = 8041, ts = 1459111051}};
            _me = new Account();
            _me.ReplaceLikes(meLikes, false);
            var otherLikes = new List<LikeStub>{new LikeStub{id=9909,ts=1458156167},new LikeStub{id=9821,ts=1487506071},new LikeStub{id=9629,ts=1452436259},new LikeStub{id=9519,ts=1458648688},new LikeStub{id=9461,ts=1507403041},new LikeStub{id=9181,ts=1538510792},new LikeStub{id=9079,ts=1464094129},new LikeStub{id=9041,ts=1455975292},new LikeStub{id=9001,ts=1532333415},new LikeStub{id=8785,ts=1486180987},new LikeStub{id=8779,ts=1477291059},new LikeStub{id=8645,ts=1539543919},new LikeStub{id=8637,ts=1471537622},new LikeStub{id=8615,ts=1524970359},new LikeStub{id=8253,ts=1488662326},new LikeStub{id=8111,ts=1531129742},new LikeStub{id=8041,ts=1516575981},new LikeStub{id=8041,ts=1457033203},new LikeStub{id=7979,ts=1458000241},new LikeStub{id=7855,ts=1468151774},new LikeStub{id=7637,ts=1526149870},new LikeStub{id=7183,ts=1460680436},new LikeStub{id=7139,ts=1483288507},new LikeStub{id=6987,ts=1500024959},new LikeStub{id=6955,ts=1454981785},new LikeStub{id=6729,ts=1528859400},new LikeStub{id=6677,ts=1459810718},new LikeStub{id=6293,ts=1526942475},new LikeStub{id=6185,ts=1456748398},new LikeStub{id=6037,ts=1497370242},new LikeStub{id=5769,ts=1518727730},new LikeStub{id=5679,ts=1534244547},new LikeStub{id=5629,ts=1528132234},new LikeStub{id=5615,ts=1456959468},new LikeStub{id=5547,ts=1455461356},new LikeStub{id=5545,ts=1534154268},new LikeStub{id=5535,ts=1535599984},new LikeStub{id=5409,ts=1526076676},new LikeStub{id=5213,ts=1462630639},new LikeStub{id=4959,ts=1480990177},new LikeStub{id=4909,ts=1534642116},new LikeStub{id=4743,ts=1469358284},new LikeStub{id=4633,ts=1457502947},new LikeStub{id=4607,ts=1501428726},new LikeStub{id=4303,ts=1533972032},new LikeStub{id=4301,ts=1456495744},new LikeStub{id=4243,ts=1533225395},new LikeStub{id=4203,ts=1474706126},new LikeStub{id=4173,ts=1500971153},new LikeStub{id=3987,ts=1459367967},new LikeStub{id=3747,ts=1474756400},new LikeStub{id=3577,ts=1469571647},new LikeStub{id=3511,ts=1503936756},new LikeStub{id=3501,ts=1508938387},new LikeStub{id=3455,ts=1510408986},new LikeStub{id=3451,ts=1465958974},new LikeStub{id=3359,ts=1514283719},new LikeStub{id=3337,ts=1492755472},new LikeStub{id=3261,ts=1467304272},new LikeStub{id=2953,ts=1486615550},new LikeStub{id=2947,ts=1492131869},new LikeStub{id=2793,ts=1462599744},new LikeStub{id=2613,ts=1490460996},new LikeStub{id=2551,ts=1469707233},new LikeStub{id=2479,ts=1501394372},new LikeStub{id=2469,ts=1462990725},new LikeStub{id=2337,ts=1535500248},new LikeStub{id=2123,ts=1460456239},new LikeStub{id=1929,ts=1509947549},new LikeStub{id=1839,ts=1453053480},new LikeStub{id=1769,ts=1523983743},new LikeStub{id=1605,ts=1508273435},new LikeStub{id=1583,ts=1461046074},new LikeStub{id=1551,ts=1456701033},new LikeStub{id=1531,ts=1491893866},new LikeStub{id=1441,ts=1487661870},new LikeStub{id=1209,ts=1492552509},new LikeStub{id=1033,ts=1489022142},new LikeStub{id=785,ts=1465524096},new LikeStub{id=781,ts=1453576449},new LikeStub{id=513,ts=1499540132},new LikeStub{id=409,ts=1518415785},new LikeStub{id=397,ts=1479956144},new LikeStub{id=283,ts=1489815483},new LikeStub{id=213,ts=1508686512},new LikeStub{id=193,ts=1474075005},new LikeStub{id=97,ts=1509885861}};
            _candidate = new Account();
            _candidate.ReplaceLikes(otherLikes, false);
        }

        [Test]
        public void Should_return_valid_data()
        {
            Similarity sim = Similarity.Of(_me, _candidate, new uint[100], out uint count );
            Similarity expected = new Similarity(3.61094677291131E-08f);
            Assert.That(sim, Is.EqualTo(expected));
        }


        [Test]
        public void Performance_tests()
        {
            Similarity sim = Similarity.Of(_me, _candidate, new uint[100], out uint count );


            const int iterations = 1_000_000;
            var watch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                Similarity.Of(_me, _candidate, new uint[100], out count );
            }

            watch.Stop();
            var opmsec = 1.0 * iterations / watch.ElapsedMilliseconds;
            Console.WriteLine(opmsec.ToString("N"));
        }

        [Test]
        public void Must_add_likes_fast()
        {
            var otherLikes = new List<LikeStub> {new LikeStub {id = 9909, ts = 1458156167}, new LikeStub {id = 9821, ts = 1487506071}, new LikeStub {id = 9629, ts = 1452436259}, new LikeStub {id = 9519, ts = 1458648688}, new LikeStub {id = 9461, ts = 1507403041}, new LikeStub {id = 9181, ts = 1538510792}, new LikeStub {id = 9079, ts = 1464094129}, new LikeStub {id = 9041, ts = 1455975292}, new LikeStub {id = 9001, ts = 1532333415}, new LikeStub {id = 8785, ts = 1486180987}, new LikeStub {id = 8779, ts = 1477291059}, new LikeStub {id = 8645, ts = 1539543919}, new LikeStub {id = 8637, ts = 1471537622}, new LikeStub {id = 8615, ts = 1524970359}, new LikeStub {id = 8253, ts = 1488662326}, new LikeStub {id = 8111, ts = 1531129742}, new LikeStub {id = 8041, ts = 1516575981}, new LikeStub {id = 8041, ts = 1457033203}, new LikeStub {id = 7979, ts = 1458000241}, new LikeStub {id = 7855, ts = 1468151774}, new LikeStub {id = 7637, ts = 1526149870}, new LikeStub {id = 7183, ts = 1460680436}, new LikeStub {id = 7139, ts = 1483288507}, new LikeStub {id = 6987, ts = 1500024959}, new LikeStub {id = 6955, ts = 1454981785}, new LikeStub {id = 6729, ts = 1528859400}, new LikeStub {id = 6677, ts = 1459810718}, new LikeStub {id = 6293, ts = 1526942475}, new LikeStub {id = 6185, ts = 1456748398}, new LikeStub {id = 6037, ts = 1497370242}, new LikeStub {id = 5769, ts = 1518727730}, new LikeStub {id = 5679, ts = 1534244547}, new LikeStub {id = 5629, ts = 1528132234}, new LikeStub {id = 5615, ts = 1456959468}, new LikeStub {id = 5547, ts = 1455461356}, new LikeStub {id = 5545, ts = 1534154268}, new LikeStub {id = 5535, ts = 1535599984}, new LikeStub {id = 5409, ts = 1526076676}, new LikeStub {id = 5213, ts = 1462630639}, new LikeStub {id = 4959, ts = 1480990177}, new LikeStub {id = 4909, ts = 1534642116}, new LikeStub {id = 4743, ts = 1469358284}, new LikeStub {id = 4633, ts = 1457502947}, new LikeStub {id = 4607, ts = 1501428726}, new LikeStub {id = 4303, ts = 1533972032}, new LikeStub {id = 4301, ts = 1456495744}, new LikeStub {id = 4243, ts = 1533225395}, new LikeStub {id = 4203, ts = 1474706126}, new LikeStub {id = 4173, ts = 1500971153}, new LikeStub {id = 3987, ts = 1459367967}, new LikeStub {id = 3747, ts = 1474756400}, new LikeStub {id = 3577, ts = 1469571647}, new LikeStub {id = 3511, ts = 1503936756}, new LikeStub {id = 3501, ts = 1508938387}, new LikeStub {id = 3455, ts = 1510408986}, new LikeStub {id = 3451, ts = 1465958974}, new LikeStub {id = 3359, ts = 1514283719}, new LikeStub {id = 3337, ts = 1492755472}, new LikeStub {id = 3261, ts = 1467304272}, new LikeStub {id = 2953, ts = 1486615550}, new LikeStub {id = 2947, ts = 1492131869}, new LikeStub {id = 2793, ts = 1462599744}, new LikeStub {id = 2613, ts = 1490460996}, new LikeStub {id = 2551, ts = 1469707233}, new LikeStub {id = 2479, ts = 1501394372}, new LikeStub {id = 2469, ts = 1462990725}, new LikeStub {id = 2337, ts = 1535500248}, new LikeStub {id = 2123, ts = 1460456239}, new LikeStub {id = 1929, ts = 1509947549}, new LikeStub {id = 1839, ts = 1453053480}, new LikeStub {id = 1769, ts = 1523983743}, new LikeStub {id = 1605, ts = 1508273435}, new LikeStub {id = 1583, ts = 1461046074}, new LikeStub {id = 1551, ts = 1456701033}, new LikeStub {id = 1531, ts = 1491893866}, new LikeStub {id = 1441, ts = 1487661870}, new LikeStub {id = 1209, ts = 1492552509}, new LikeStub {id = 1033, ts = 1489022142}, new LikeStub {id = 785, ts = 1465524096}, new LikeStub {id = 781, ts = 1453576449}, new LikeStub {id = 513, ts = 1499540132}, new LikeStub {id = 409, ts = 1518415785}, new LikeStub {id = 397, ts = 1479956144}, new LikeStub {id = 283, ts = 1489815483}, new LikeStub {id = 213, ts = 1508686512}, new LikeStub {id = 193, ts = 1474075005}, new LikeStub {id = 97, ts = 1509885861}};
            _candidate = new Account();

            var watch = Stopwatch.StartNew();
            for (int i = 0; i < 100_000; i++)
            {
                _candidate.ReplaceLikes(otherLikes, false);
            }
            watch.Stop();

            Console.WriteLine(watch.ElapsedMilliseconds);


        }

    }
}