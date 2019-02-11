using System;
using System.Collections.Generic;
using System.IO;
using aspnetapp;
using aspnetapp.Collections;
using aspnetapp.Serializer;
using NUnit.Framework;

namespace Tests
{
    public class FastJsonTests
    {
        [Test]
        public void Should_serialize_specified_fields()
        {
/*            var accounts = new HList<Account>
            {
                new Account
                {
                    id = 1,
                    Email = new Email("my", 1),
                    city = "London"
                }
            };

            var stream = new MemoryStream();
            var props = new HashSet<int> {Fields.City};

            FastJson.WriteFilterResponse(accounts, props, stream);

            stream.Position = 0;
            var reader = new StreamReader(stream);

            var result = reader.ReadToEnd();*/

            //Console.WriteLine(result);
        }
    }
}