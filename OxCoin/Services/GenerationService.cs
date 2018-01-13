using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using System.Linq;
using System.Collections.Generic;
using OxCoin.Repository;
using OxCoin.Repository.Entities;

namespace OxCoin.Services
{
    public class GenerationService : IStartable, IDisposable
    {
        private const int Sleep = 500;
        private static int _counter = 0;
        private readonly CancellationTokenSource _cancelSource = new CancellationTokenSource();
        private CancellationToken _cancelToken;

        private Task _monitoringTask;

        public void Start()
        {
            _cancelToken = _cancelSource.Token;

            _monitoringTask = new Task(Run, TaskCreationOptions.LongRunning);
            _monitoringTask.Start();
        }

        private void Run()
        {
            if (GetGenesisUser() == null)
            {
                GenerateGenesisUserWithWalletId();
                GenerateUsersWithWalletIds();
                GenerateMinersWithWalletIds();
            }

            Thread.Sleep(100);
            Console.WriteLine();
            Console.WriteLine("Generating transactions...");
            Console.WriteLine();
            Console.Write("Press any key to exit...");
            Console.WriteLine();
            Console.WriteLine();
            Console.CursorVisible = false;

            //var spinner = new ConsoleSpinner();

            while (!_cancelToken.IsCancellationRequested)
            {
                _counter++;

                Console.Write("\rTransactions generated: {0}                      ", _counter);
                //spinner.Turn();
                GenerateTransaction();

                //WaitHandle.WaitAny(new[] { cancelToken.WaitHandle }, TimeSpan.FromSeconds(30));
            }
        }

        public void Dispose()
        {
            _cancelSource.Cancel();
            _monitoringTask.Wait();
        }

        private static User GetGenesisUser()
        {
            using (var db = new OxCoinDbContext())
            {
                return db.Users.FirstOrDefault(x => x.GivenName == "Network" && x.FamilyName == "Admin");
            }
        }

        private static void GenerateGenesisUserWithWalletId()
        {
            var genesisUser = new User
            {
                GivenName = "Network",
                FamilyName = "Admin"
            };

            Console.WriteLine("Creating Genesis user...");

            AddUser(genesisUser);

            Thread.Sleep(Sleep);

            Console.WriteLine("Creating Genesis wallet...");

            AddWallet(new Wallet { UserId = GetGenesisUser().Id });

            Thread.Sleep(Sleep);
        }

        private static void AddUser(User genesisUser)
        {
            using (var db = new OxCoinDbContext())
            {
                db.Users.Add(genesisUser);
                db.SaveChanges();
            }
        }

        private static void AddWallet(Wallet wallet)
        {
            using (var db = new OxCoinDbContext())
            {
                db.Wallets.Add(wallet);
                db.SaveChanges();
            }
        }

        private static void GenerateUsersWithWalletIds()
        {
            var users = new List<User>
            {
                new User { GivenName = "Alistair", FamilyName = "Evans" },
                new User { GivenName = "Owain", FamilyName = "Richardson" },
                new User { GivenName = "Matt", FamilyName = "Stahl-Coote" },
                new User { GivenName = "Chris", FamilyName = "Bedwell" },
                new User { GivenName = "Cris", FamilyName = "Oxley" },
                new User { GivenName = "Luke", FamilyName = "Hunt" },
                new User { GivenName = "Tracey", FamilyName = "Young" },
                new User { GivenName = "Dan", FamilyName = "Blackmore" },
                new User { GivenName = "Craig", FamilyName = "Jenkins" },
                new User { GivenName = "John", FamilyName = "Rudden" }
            };

            Console.WriteLine("Creating test users and wallets...");

            foreach (var user in users)
            {
                if (GetUsers().FirstOrDefault(x => x.FamilyName == user.FamilyName && x.GivenName == user.GivenName) != null)
                {
                    break;
                }

                AddUser(user);
                AddWallet(new Wallet { UserId = user.Id });
            }

            Thread.Sleep(Sleep);
        }

        private static IEnumerable<User> GetUsers()
        {
            using (var db = new OxCoinDbContext())
            {
                foreach (var user in db.Users.Where(x => x.Id != GetGenesisUser().Id))
                {
                    yield return user;
                }
            }
        }

        private static void GenerateMinersWithWalletIds()
        {
            Console.WriteLine("Creating miners and wallets...");

            foreach (var user in GetUsers())
            {
                var wallet = new Wallet
                {
                    UserId = user.Id
                };

                AddWallet(wallet);

                var miner = new Miner
                {
                    WalletId = wallet.Id
                };

                AddMiner(miner);
            }

            Thread.Sleep(Sleep);
        }

        private static void AddMiner(Miner miner)
        {
            using (var db = new OxCoinDbContext())
            {
                db.Miners.Add(miner);
                db.SaveChanges();
            }
        }

        private static void GenerateTransaction()
        {
            var random = new Random();
            var sourceWalletId = GetRandomWalletId();
            var destinationWalletId = GetRandomWalletId(sourceWalletId);

            AddTransaction(new Transaction
            {
                SourceWalletId = sourceWalletId,
                DestinationWalletId = destinationWalletId,
                TransferedAmount = Math.Round(random.Next(1, 999) / 100000.1m, 4),
                Timestamp = new DateTime(random.Next(2017, 2017), random.Next(1, 12), random.Next(1, 28), random.Next(0, 23), random.Next(0, 59), random.Next(0, 59), random.Next(0, 999))
            });
        }

        private static Guid GetRandomWalletId(Guid? idToExclude = null)
        {
            var miners = GetMiners().ToList();
            var wallets = GetWallets().Where(x => x.Id != idToExclude &&
                                                  x.UserId != GetGenesisUser().Id)
                                      .ToList();

            foreach (var miner in miners)
            {
                var wallet = wallets.First(x => x.Id == miner.WalletId);

                wallets.Remove(wallet);
            }

            wallets.Shuffle();

            return wallets[new Random().Next(0, wallets.Count - 1)].Id;
        }

        private static void AddTransaction(Transaction transaction)
        {
            using (var db = new OxCoinDbContext())
            {
                db.Transactions.Add(transaction);
                db.SaveChanges();
            }
        }

        private static IEnumerable<Miner> GetMiners()
        {
            using (var db = new OxCoinDbContext())
            {
                foreach (var miner in db.Miners)
                {
                    yield return miner;
                }
            }
        }

        private static IEnumerable<Wallet> GetWallets()
        {
            using (var db = new OxCoinDbContext())
            {
                foreach (var wallet in db.Wallets.Where(x => x.UserId != GetGenesisUser().Id))
                {
                    yield return wallet;
                }
            }
        }
    }

    #region Etcetera
    public class ConsoleSpinner
    {
        private int _counter = 0;

        private const int Delay = 100;
        private readonly string[] _sequence = { "/", "-", "\\", "|" };

        public void Turn()
        {
            _counter++;

            if (_counter >= _sequence.Length)
            {
                _counter = 0;
            }

            Console.Write("\r" + _sequence[_counter]);

            Thread.Sleep(Delay);
        }
    }

    public static class ThreadSafeRandom
    {
        [ThreadStatic] private static Random _local;

        public static Random ThisThreadsRandom => _local ?? (_local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId)));
    }

    internal static class ListExtensions
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
    #endregion
}
