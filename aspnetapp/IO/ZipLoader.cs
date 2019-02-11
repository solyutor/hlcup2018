using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using aspnetapp.Controllers;
using Newtonsoft.Json;

namespace aspnetapp.Loader
{
    public class ZipLoader : IDisposable
    {
        private readonly string _path;
        private readonly JsonSerializer _serializer;
        private readonly FileStream _stream;
        private readonly ZipArchive _zip;

        public ZipLoader(string path)
        {
            _path = path;
            _serializer = new JsonSerializer();
            _stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            _zip = new ZipArchive(_stream, ZipArchiveMode.Read);

        }

        public int GetCount()
        {
            return _zip.Entries.Count * 10_000;
        }

        public int GetNow()
        {
            var folder = Path.GetDirectoryName(_path);
            var path = Path.Combine(folder, "options.txt");

            using (Stream stream = File.OpenRead(path))
            using (var reader = new StreamReader(stream))
            {
                return int.Parse(reader.ReadLine());
            }
        }

        public void GetAccounts(BlockingCollection<AccountStub> collection)
        {
            IEnumerable<string> accountEntries = _zip
                .Entries
                .OrderBy(x =>
                    int.Parse(
                        x.Name
                            .Replace("accounts_", string.Empty)
                            .Replace(".json", String.Empty))
                ).Where(x => x.FullName.StartsWith("accounts_", StringComparison.OrdinalIgnoreCase))
                .Select(x => x.FullName);

            var queue = new ConcurrentQueue<string>(accountEntries);

            Task.WhenAll(
                    Task.Run(() => Process(queue, collection)),
                    Task.Run(() => Process(queue, collection)),
                    Task.Run(() => Process(queue, collection)),
                    Task.Run(() => Process(queue, collection)))
                .ContinueWith(t => collection.CompleteAdding());
        }


        private void Process(ConcurrentQueue<string> entries, BlockingCollection<AccountStub> collection)
        {
            var memory = new MemoryStream(1024 * 1024);

            using(var stream = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 1024))
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Read))
            {
                while (entries.TryDequeue(out string entryName))
                {
                    memory.Position = 0;

                    ZipArchiveEntry zipEntry = zip.GetEntry(entryName);
                    using (var entryStream = zipEntry.Open())
                    {
                        entryStream.CopyTo(memory);
                    }

                    memory.Position = 0;
                    Deserialiaze(memory, collection);
                }
            }

        }

        private void Deserialiaze(Stream stream, BlockingCollection<AccountStub> collection)
        {
            using (var reader = new JsonTextReader(new StreamReader(stream)) {SupportMultipleContent = true, CloseInput = false})
            {
                while (reader.TokenType != JsonToken.StartArray)
                {
                    reader.Read();
                }

                reader.Read();

                do
                {
                    while (reader.TokenType != JsonToken.StartObject)
                    {
                        reader.Read();
                    }

                    collection.Add(_serializer.Deserialize<AccountStub>(reader));

                    reader.Read();
                } while (reader.TokenType != JsonToken.EndArray);
            }
        }

        public IEnumerable<AccountStub> GetAccounts()
        {
            IEnumerable<ZipArchiveEntry> accountEntries = _zip
                .Entries
                .OrderBy(x =>
                    int.Parse(
                        x.Name
                            .Replace("accounts_", string.Empty)
                            .Replace(".json", String.Empty))
                ).Where(x => x.FullName.StartsWith("accounts_", StringComparison.OrdinalIgnoreCase));

            foreach (ZipArchiveEntry entry in accountEntries)
            {
                using (Stream stream = entry.Open())
                using (var reader = new JsonTextReader(new StreamReader(stream)) {SupportMultipleContent = true})
                {
                    while (reader.TokenType != JsonToken.StartArray)
                    {
                        reader.Read();
                    }

                    reader.Read();

                    do
                    {
                        while (reader.TokenType != JsonToken.StartObject)
                        {
                            reader.Read();
                        }

                        yield return _serializer.Deserialize<AccountStub>(reader);

                        reader.Read();
                    } while (reader.TokenType != JsonToken.EndArray);
                }
            }
        }


        public void Dispose()
        {
            _zip?.Dispose();
            _stream?.Dispose();
        }
    }
}
