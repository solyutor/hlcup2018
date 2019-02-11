using System;
using System.Collections.Generic;
using System.Text;
using aspnetapp;
using aspnetapp.Controllers;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Tests
{
    public class NewHandlerTests
    {
        [Test]
        public unsafe void Should_parse_json()
        {
            var body = new
            {
                id = 123,
                email = "someone@domaim.com",
                status = @"\u0432\u0441\u0451 \u0441\u043b\u043e\u0436\u043d\u043e",
                sex = "m",
                fname = "Jury",
                sname = "Soldatenkov",

                interests = new[] {"sex", "drugs", "rock'n'roll"},

                likes = new object[]
                {
                    new {id = 245, ts = 123533214},
                    new {id = 123, ts = 123532514},
                },
                premium = new
                {
                    start = 1000,
                    finish = 2000
                }
            };
            var stringBody = JsonConvert.SerializeObject(body);
            byte[] bytes = Encoding.UTF8.GetBytes(stringBody);

            fixed (byte* p = &bytes[0])
            {
                ParseContext pctx =  new ParseContext(p, bytes.Length);
                byte[] idBytes = Encoding.UTF8.GetBytes("123");
                Console.WriteLine(stringBody);
                Console.WriteLine((byte)',');
                Console.WriteLine(string.Join(" ", bytes));
                Console.WriteLine(string.Join(" ", idBytes));

                var account = new AccountStub();
                var result = NewHandler.TryParseAccount(ref pctx, account);

                Assert.That(result, Is.EqualTo(201));
                Assert.That(account.id, Is.EqualTo(body.id));
                Assert.That(account.email, Is.EqualTo(body.email));
                Assert.That(account.sexStatus.IsMale(), Is.True);
                //Assert.That(account.fname, Is.EqualTo(body.fname));
                Assert.That(account.sname, Is.EqualTo(body.sname));

                //CollectionAssert.AreEquivalent(account.interests, body.interests);
                Assert.That(account.likes.Count, Is.EqualTo(body.likes.Length));
                Assert.That(account.premium.start, Is.EqualTo(body.premium.start));
                Assert.That(account.premium.finish, Is.EqualTo(body.premium.finish));
            }
        }

        [TestCase("{\"birth\":596976776,\"city\":\"Мосостан\",\"country\":\"Герания\",\"email\":\"ratrawtadroteonir@yandex.ru\",\"fname\":\"Милена\",\"id\":10002,\"interests\":[\"Целоваться\",\"50 Cent\",\"Горы\"],\"joined\":1326585600,\"likes\":[{\"id\":3873,\"ts\":1540838332},{\"id\":9899,\"ts\":1466781540},{\"id\":9045,\"ts\":1456229580},{\"id\":5805,\"ts\":1535021435},{\"id\":8027,\"ts\":1493845573},{\"id\":7753,\"ts\":1511695685},{\"id\":695,\"ts\":1491204594},{\"id\":1513,\"ts\":1513187217},{\"id\":3651,\"ts\":1528435354},{\"id\":7885,\"ts\":1537901867},{\"id\":6177,\"ts\":1478308087},{\"id\":8943,\"ts\":1466917427},{\"id\":3947,\"ts\":1521758436}],\"sex\":\"f\",\"status\":\"свободны\"}")]
        public unsafe void ShouldParse(string json)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            fixed (byte* p = bytes)
            {
                var pctx = new ParseContext(p, bytes.Length);
                var stub = new AccountStub();
                Assert.That(NewHandler.TryParseAccount(ref pctx, stub), Is.EqualTo(201));
            }
        }

        [Test]
        public void Decode_Unicode()
        {
            Console.WriteLine($"Max age diff: {TimeStamp.MaxAgeDiff}");
            Console.WriteLine($"Free value to compare: {(int.MaxValue - TimeStamp.MaxAgeDiff).ToString("N")}");


            Console.WriteLine(DateTime.UnixEpoch.AddSeconds(1452384000));
            Console.WriteLine(DateTime.UnixEpoch.AddSeconds(1541807999));
            var i = 1541807999 - TimeStamp.MinJoined;
            Console.WriteLine(i.ToString("N2"));
            Console.WriteLine(uint.MaxValue / i);
            Console.WriteLine(uint.MaxValue / (TimeStamp.MaxJoined - TimeStamp.MinJoined));

            var list = new SortedList<int, int>();
            var set = new SortedSet<Like>();
            set.GetViewBetween(new Like(), new Like());
        }
    }
}