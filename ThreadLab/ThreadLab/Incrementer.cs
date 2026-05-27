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

        private static IncrementerRepository _incrementerRepository = new IncrementerRepository();

        private static int _setCount = 0;
        private static int _waitCount = 0;

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
            stopWatch.Restart();
            Console.WriteLine("All worker threads started. Time elapsed = " + stopWatch.Elapsed.TotalSeconds);

            //Wait for the threads to be completed
            for (var i = 0; i < threads.Length; i++)
            {
                threads[i].Join();
            }

            _incrementerRepository.UpdateThreadJobDateTimeFinished(threadJobId);
            Console.WriteLine("All threads are done. Time elapsed = " + stopWatch.Elapsed.TotalSeconds + ". SetCount = " + _setCount + ", WaitCount = " + _waitCount);
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

                //remove
                if (_newIncrementEnd >= int.MaxValue)
                {
                    break;
                }

                //remove
                if (_newIncrementEnd >= int.MaxValue)
                {
                    break;
                }

                threads.Add(new Thread(() =>
                {
                    var localStart = start;
                    var localEnd = end;

                    if (localStart >= int.MaxValue || localEnd >= int.MaxValue)
                    {
                        var t = "";
                    }

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

                //remove
                if (incrementStart < 0 || incrementEnd < 0 || incrementStart >= int.MaxValue || incrementEnd >= int.MaxValue)
                {
                    var t = "1";
                }

                //Here is the core of the experiment; incrementing by one in each cycle
                for (var i = incrementStart; i < incrementEnd; i++);

                _incrementerRepository.UpdateThreadIterationDateTimeFinished(threadIterationId);
                if (WaitHandle.WaitAny(new WaitHandle[] { _workerWaitHandle, token.WaitHandle }) != 0)
                {
                    break;
                }

                //remove
                if (_newIncrementStart < 0 || _newIncrementEnd < 0 || _newIncrementStart >= int.MaxValue || _newIncrementEnd >= int.MaxValue)
                {
                    var t = "1";
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

                //remove
                if (_newIncrementStart < 0 || _newIncrementEnd < 0 || _newIncrementStart >= int.MaxValue || _newIncrementEnd >= int.MaxValue)
                {
                    var t = "1";
                }

                if (_newIncrementStart >= int.MaxValue)
                {
                    cancellationTokenSource.Cancel();

                    break;
                }

                IncrementWaitCount();

                _workerWaitHandle.Set();
                _mainWaitHandle.WaitOne();

                Console.WriteLine("Counting to " + _newIncrementStart);
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