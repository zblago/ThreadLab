using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using ThreadLab.Database;
using ThreadLab.Utility;

namespace ThreadLab
{
    internal class Incrementer
    {
        private static EventWaitHandle _workerWaitHandle = new AutoResetEvent(false);

        private static EventWaitHandle _mainWaitHandle = new AutoResetEvent(false);
        private static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private static CancellationToken token = cancellationTokenSource.Token;

        private static long _newIncrementStart = 0;
        private static long _newIncrementEnd = 0;
        private static int _numberOfIncrementsPerThread = 0;
        private static int _exitedThreadCount = 0;

        private static IIncrementerRepository _incrementerRepository;

        private static int _setCount = 0;
        private static int _waitCount = 0;

        static Incrementer()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var storageType = Enum.Parse<StorageType>(configuration.GetValue<string>("StorageType"));
            if (storageType == StorageType.InMemoryEfDbContext || storageType == StorageType.SQLServerEfDbContext)
            {
                _incrementerRepository = new IncrementerRepository();
            }
            else
            { 
                _incrementerRepository = new IncrementerMemoryRepository();
            }
        }

        public static void Run(int numberOfThreads, int numberOfIncrementsPerThread)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            _numberOfIncrementsPerThread = numberOfIncrementsPerThread;

            var threadJobId = _incrementerRepository.AddThreadJob(CurrentThreadInfo.CurrentManagedThreadId, CurrentThreadInfo.IsBackgroundThread, numberOfThreads, numberOfIncrementsPerThread);
            stopWatch.Restart();

            Console.WriteLine("Creating worker threads...");
            var threads = CreateWorkerThreads(numberOfThreads, numberOfIncrementsPerThread, threadJobId);
            if (threads.Length == 0)
            {
                return;
            }
            Console.WriteLine($"Worker threads created." +
                $"{(threads.Count() < numberOfThreads ? $" {threads.Count()} threads created since not available range for reminaing ones." +
                $"Consider fine tuning input parameters" : string.Empty)}");

            //Create and start main thread            
            Thread mainThread = new Thread(x => MainThreadJob());
            mainThread.Start();
            Console.WriteLine("Main thread started. Time elapsed = " + stopWatch.Elapsed.TotalSeconds + ". Starting worker threads...");

            //Create and start worker threads
            for (var i = 0; i < threads.Length; i++)
            {
                threads[i].Start();
            }

            //Wait for the threads to be completed
            for (var i = 0; i < threads.Length; i++)
            {
                threads[i].Join();
            }

            _incrementerRepository.UpdateThreadJobDateTimeFinished(threadJobId);
            stopWatch.Stop();
            Console.WriteLine("All threads are done. Time elapsed = " + stopWatch.Elapsed.TotalSeconds + ". SetCount = " + _setCount + ", WaitCount = " + _waitCount + ",  ThreadCount = " + numberOfThreads + ", Batch size (number of increments per batch) = " + _numberOfIncrementsPerThread);
            Console.WriteLine("Running tests now...");

            var hasDuplicateStartNumber = _incrementerRepository.HasDuplicateStartNumber(threadJobId);
            var hasDuplicateEndNumber = _incrementerRepository.HasDuplicateEndNumber(threadJobId);
            var hasGaps = _incrementerRepository.HasGaps(threadJobId);

            if (hasDuplicateStartNumber || hasDuplicateEndNumber || hasGaps)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Test failed.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Test passed.");
            }
        }

        private static Thread[] CreateWorkerThreads(int numberOfThreads, int numberOfIncrementsPerThread, int threadJobId)
        {
            var threads = new List<Thread>();

            for (var i = 0; i < numberOfThreads; i++)
            {
                var start = (long)(i * numberOfIncrementsPerThread);
                var end = (long)((i + 1) * numberOfIncrementsPerThread);

                if (end >= int.MaxValue || end < 0)
                {
                    break;
                }

                if (start > int.MaxValue || end > int.MaxValue)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Initial range for a first run exceeded. Choose lower numbers e.g. Number of threads = 500, Batch size = 1000000.");

                    return Array.Empty<Thread>();
                }

                //Initial value for the next batch.
                _newIncrementStart = start;
                _newIncrementEnd = end;

                threads.Add(new Thread(() =>
                {
                    var localStart = start;
                    var localEnd = end;

                    WorkerThreadJob(localStart, localEnd, threadJobId);
                }));
            }

            return threads.ToArray();
        }

        private static void WorkerThreadJob(long incrementStart, long incrementEnd, int threadJobId)
        {
            while (true)
            {
                var threadIterationId =
                    _incrementerRepository.AddThreadIteration(threadJobId, CurrentThreadInfo.CurrentManagedThreadId, CurrentThreadInfo.IsBackgroundThread, incrementStart, incrementEnd);

                Console.WriteLine("Counting to " + incrementEnd);
                //Here is the core of the experiment; incrementing by one in each cycle
                for (var i = incrementStart; i < incrementEnd; i++)
                {
                    var hallo = "hallo";
                }
                Console.WriteLine("Counting to " + incrementEnd + " completed");

                _incrementerRepository.UpdateThreadIterationDateTimeFinished(threadIterationId);
                if (WaitHandle.WaitAny(new WaitHandle[] { _workerWaitHandle, token.WaitHandle }) != 0)
                {
                    ++_exitedThreadCount;
                    Console.WriteLine($"Exiting the thread, {_exitedThreadCount}");
                    break;
                }

                incrementStart = _newIncrementStart;
                incrementEnd = _newIncrementEnd;

                _mainWaitHandle.Set();

                IncrementSetCount();
            }
        }

        private static void MainThreadJob()
        {
            while (true)
            {
                _newIncrementStart = _newIncrementEnd;
                _newIncrementEnd = (long)_newIncrementStart + (long)_numberOfIncrementsPerThread > int.MaxValue
                    ? int.MaxValue
                    : _newIncrementStart + _numberOfIncrementsPerThread;

                if (_newIncrementStart >= int.MaxValue)
                {
                    cancellationTokenSource.Cancel();

                    break;
                }

                IncrementWaitCount();

                _workerWaitHandle.Set();
                _mainWaitHandle.WaitOne();
            }
        }

        private static void IncrementSetCount()
        {
            ++_setCount;
        }

        private static void IncrementWaitCount()
        {
            ++_waitCount;
        }
    }
}