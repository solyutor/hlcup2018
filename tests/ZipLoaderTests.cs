using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using aspnetapp;
using aspnetapp.Controllers;
using aspnetapp.Loader;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Tests
{
    public class ZipLoaderTests
    {
        [Test]
        public void Should_unpack_all_accounts()
        {
            var file = Path.GetFullPath("data.zip");

            var loader = new ZipLoader(file);

            Stopwatch watch = Stopwatch.StartNew();

            var count = loader.GetAccounts().Count();

            watch.Stop();

            Console.WriteLine($"Took {watch.ElapsedMilliseconds.ToString()}");

            Assert.That(count, Is.EqualTo(10_000));
        }

        [Test]
        [Ignore("Use it make a few files in a an archive")]
        public void Split_data_zip()
        {
            var file = Path.GetFullPath("data.zip");

            var loader = new ZipLoader(file);

            Stopwatch watch = Stopwatch.StartNew();

            var count = 0;
            List<AccountStub> accounts = loader.GetAccounts().ToList();
            var serializer = new JsonSerializer();

            using (FileStream fs = File.OpenWrite(Path.GetFullPath("accounts_1.json")))
            using (var writer = new JsonTextWriter(new StreamWriter(fs)))
            {
                serializer.Serialize(writer, new {accounts = accounts.Take(5000)});
            }

            using (FileStream fs = File.OpenWrite(Path.GetFullPath("accounts_2.json")))
            using (var writer = new JsonTextWriter(new StreamWriter(fs)))
            {
                serializer.Serialize(writer, new {accounts = accounts.Skip(5000)});
            }

            watch.Stop();

            Console.WriteLine($"Took {watch.ElapsedMilliseconds.ToString()}");

            Assert.That(count, Is.EqualTo(10_000));
        }
    }
}