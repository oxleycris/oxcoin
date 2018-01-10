using System;
using System.Threading;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace OxCoin
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var cancelSource = new CancellationTokenSource();
            var appBackground = BuildWebHost(args).RunAsync(cancelSource.Token);

            Console.WriteLine("Press any key to DIE...");
            Console.ReadLine();

            cancelSource.Cancel();

            appBackground.Wait();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
