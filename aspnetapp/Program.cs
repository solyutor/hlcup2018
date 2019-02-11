using System;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime;
using System.Threading.Tasks;
using aspnetapp.Controllers;
using aspnetapp.Serializer;
using aspnetapp.Sys;
using Microsoft.AspNetCore.Hosting;

namespace aspnetapp
{
    public class Program
    {
        public static int Main(string[] args)
        {
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

            try
            {
                IWebHost host = InitializationWorker.Run(args.Length == 0 ? null :  args[0]).Result;
                Console.WriteLine("Initialization Finished");
                Collect(true);

                WarmUpWorker.Run();

                Collect(true);
                Collect(true);
                Collect(true);

                //Stats.Print();
                UnsafeStringContainer.PrintUsage();
                host.WaitForShutdown();

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return -1;
            }

            Console.WriteLine("Application exited");
            return 0;
        }

        public static void Collect(bool forceLohCompaction = false)
        {
            var before = Environment.WorkingSet/ 1024.0 / 1024.0;

            if (forceLohCompaction)
            {
                GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            }
            GC.WaitForPendingFinalizers();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
            var after = Environment.WorkingSet/ 1024.0 / 1024.0;

            Console.WriteLine($"Working set: before={before:N2} after={after:N2}");

        }
    }
}