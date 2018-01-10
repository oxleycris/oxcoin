using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;

namespace OxCoin.Services
{
    public class RetardService : IStartable, IDisposable
    {
        private readonly CancellationTokenSource cancelSource = new CancellationTokenSource();
        private CancellationToken cancelToken;

        private Task monitoringTask;

        public void Start()
        {
            cancelToken = cancelSource.Token;

            monitoringTask = new Task(Run, TaskCreationOptions.LongRunning);
            monitoringTask.Start();
        }

        private void Run()
        {
            var pass = 0;

            while (!cancelToken.IsCancellationRequested)
            {
                pass++;

                Console.WriteLine("Pass: " + pass);

                for (int i = 0; i < 10; i++)
                {
                    Console.WriteLine("Number " + i);
                }

                // Using WaitAny because eventually I want to be able to trigger an immediate config refresh.
                WaitHandle.WaitAny(new[] { cancelToken.WaitHandle }, TimeSpan.FromSeconds(30));
            }
        }

        public void Dispose()
        {
            cancelSource.Cancel();
            monitoringTask.Wait();
        }
    }
}
