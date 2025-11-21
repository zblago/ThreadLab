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

        private static object _lock = new object();
        private static int _setCount = 0;
        private static int _waitCount = 0;

        public static void Run(int numberOfThreads, int numberOfIncrementsPerThread)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            _numberOfIncrementsPerThread = numberOfIncrementsPerThread;

            var threadJobId = _incrementerRepository.AddThreadJob(CurrentThreadInfo.CurrentManagedThreadId, CurrentThreadInfo.IsBackgroundThread, numberOfThreads, numberOfIncrementsPerThread);
            Console.WriteLine("Thread Job record created. Time elapsed = " + stopWatch.Elapsed.TotalSeconds);
            stopWatch.Restart();

            var threads = CreateWorkerThreads(numberOfThreads, numberOfIncrementsPerThread, threadJobId);
            Console.WriteLine("Worker threads created. Time elapsed = " + stopWatch.Elapsed.TotalSeconds);
            stopWatch.Restart();

            //Create and start main thread
            Thread mainThread = new Thread(x => MainThreadJob());
            mainThread.Start();

            //Create and start worker threads
            for (var i = 0; i < threads.Length; i++)
            {
                threads[i].Start();
            }
            stopWatch.Restart();

            //Wait for the threads to be completed
            for (var i = 0; i < threads.Length; i++)
            {
                threads[i].Join();
            }

            _incrementerRepository.UpdateThreadJobDateTimeFinished(threadJobId);
            Console.WriteLine("All threads are done. Time elapsed = " + stopWatch.Elapsed.TotalSeconds);
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
            var threads = new Thread[numberOfThreads];

            for (var i = 0; i < numberOfThreads; i++)
            {
                var start = (long)(i * numberOfIncrementsPerThread);
                var end = (long)((i + 1) * numberOfIncrementsPerThread);

                //Initial value for the second batch.
                _newIncrementStart = start;
                _newIncrementEnd = end;

                threads[i] = new Thread(() => {
                    var localStart = start;
                    var localEnd = end;
                    WorkerThreadJob(localStart, localEnd, threadJobId);
                });
            }

            return threads;
        }

        private static void WorkerThreadJob(long incrementStart, long incrementEnd, int threadJobId)
        {
            while (true)
            {
                var threadIterationId =
                    _incrementerRepository.AddThreadIteration(threadJobId, CurrentThreadInfo.CurrentManagedThreadId, CurrentThreadInfo.IsBackgroundThread, incrementStart, incrementEnd);

                //Here is the core of the experiment; incrementing by one in each cycle
                for (var i = incrementStart; i < incrementEnd; i++) ;

                _incrementerRepository.UpdateThreadIterationDateTimeFinished(threadIterationId);

                if (WaitHandle.WaitAny(new WaitHandle[] { _workerWaitHandle, token.WaitHandle }) != 0)
                {
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
                _newIncrementEnd = _newIncrementStart + (long)_numberOfIncrementsPerThread;

                var maxValue = (long)int.MaxValue; //Don't go over long
                if (_newIncrementEnd > maxValue)
                {
                    _newIncrementEnd = maxValue;

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
            lock (_lock)
            {
                ++_setCount;
            }
        }

        private static void IncrementWaitCount()
        {
            lock (_lock)
            {
                ++_waitCount;
            }
        }
    }
}