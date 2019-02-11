using System;
using System.Threading.Tasks;
using aspnetapp.Controllers;
using aspnetapp.Serializer;
using Microsoft.AspNetCore.Hosting;

namespace aspnetapp
{
    public static class InitializationWorker
    {
        public static async Task<IWebHost> Run(string dbName)
        {

            Task<IWebHost> hostTask = Task.Run(InitializeWebServer);

            await Task.WhenAll(
                Task.Run(() => InitializeDatabase(dbName)),
                Task.Run(CompilePredicates),
                Task.Run(UpdateWorker.Init),
                Task.Run(UpdatePool.Initialize),
                hostTask)
                .ConfigureAwait(false);

            return hostTask.Result;

        }

        private static void CompilePredicates()
        {
            try
            {
                FilterBuilder.GetFilter(FilterTypes.empty);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        private static IWebHost InitializeWebServer()
        {
            IWebHost host = new WebHostBuilder()
                .UseKestrel(c =>
                {
                    c.AddServerHeader = false;
                    c.ListenAnyIP(80);
/*                        c.Limits.MaxResponseBufferSize = 64 * 1024;
                        c.Limits.MinRequestBodyDataRate = null;*/
                    c.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(1);
                })
                .UseLinuxTransport()
                .UseStartup<Startup>()
                .Build();

            host.Start();
            return host;
        }

        private static void InitializeDatabase(string dataFile)
        {
            Database.Init(dataFile);
        }
    }
}