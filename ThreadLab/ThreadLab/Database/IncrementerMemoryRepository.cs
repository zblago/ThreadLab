namespace ThreadLab.Database
{
    internal class IncrementerMemoryRepository : IIncrementerRepository
    {
        private static readonly object _threadJobLock = new();

        private readonly List<ThreadJob> _threadJobs = new();
        private readonly List<ThreadIteration> _threadIterations = new();

        private int _nextThreadJobId = 1;
        private int _nextThreadIterationId = 1;

        public IReadOnlyList<ThreadJob> ThreadJobs => _threadJobs;
        public IReadOnlyList<ThreadIteration> ThreadIterations => _threadIterations;

        public int AddThreadJob(
            int managedThreadId,
            bool isBackground,
            int numberOfThreads,
            int numberOfStepsPerThread)
        {
            var threadJob = new ThreadJob
            {
                ThreadJobId = _nextThreadJobId++,
                ManagedThreadId = managedThreadId,
                IsBackground = isBackground,
                NumberOfThreads = numberOfThreads,
                NumberOfStepsPerThread = numberOfStepsPerThread,
                DateTimeStarted = DateTime.UtcNow
            };

            lock (_threadJobLock)
            {
                _threadJobs.Add(threadJob);
            }

            return threadJob.ThreadJobId;
        }

        public void UpdateThreadJobDateTimeFinished(int threadJobId)
        {
            lock (_threadJobLock)
            {
                var threadJob = _threadJobs.Single(x => x.ThreadJobId == threadJobId);
                threadJob.DateTimeFinished = DateTime.UtcNow;
            }
        }

        public bool HasDuplicateStartNumber(int threadJobId)
        {
            lock (_threadJobLock)
            {
                return _threadIterations
                    .Where(x => x.ThreadJobId == threadJobId)
                    .GroupBy(x => x.StartNumber)
                    .Any(x => x.Count() > 1);
            }
        }

        public bool HasDuplicateEndNumber(int threadJobId)
        {
            lock (_threadJobLock)
            {
                return _threadIterations
                    .Where(x => x.ThreadJobId == threadJobId)
                    .GroupBy(x => x.EndNumber)
                    .Any(x => x.Count() > 1);
            }
        }

        public bool HasGaps(int threadJobId)
        {
            lock (_threadJobLock)
            {
                var threadJob = _threadJobs
                    .Single(x => x.ThreadJobId == threadJobId);

                var iterations = _threadIterations
                    .Where(x => x.ThreadJobId == threadJobId)
                    .OrderBy(x => x.StartNumber)
                    .ToList();

                if (iterations.Count == 0)
                    return false;

                var lastIterationId = iterations.Max(x => x.ThreadIterationId);

                for (var i = 0; i < iterations.Count; i++)
                {
                    var iteration = iterations[i];

                    var calculatedStartNumber =
                        i * (long)threadJob.NumberOfStepsPerThread;

                    var isLastIteration =
                        iteration.ThreadIterationId == lastIterationId;

                    var calculatedEndNumber = isLastIteration
                        ? long.MaxValue
                        : calculatedStartNumber + threadJob.NumberOfStepsPerThread;

                    if (isLastIteration)
                        return false;

                    if (calculatedStartNumber != iteration.StartNumber ||
                        calculatedEndNumber != iteration.EndNumber)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public int AddThreadIteration(
            int threadJobId,
            int managedThreadId,
            bool isBackground,
            long startNumber,
            long endNumber)
        {
            var threadIteration = new ThreadIteration
            {
                ThreadIterationId = _nextThreadIterationId++,
                ThreadJobId = threadJobId,
                ManagedThreadId = managedThreadId,
                IsBackground = isBackground,
                StartNumber = startNumber,
                EndNumber = endNumber,
                DateTimeStarted = DateTime.UtcNow
            };

            lock (_threadJobLock)
            {
                _threadIterations.Add(threadIteration);
            }

            return threadIteration.ThreadIterationId;
        }

        public void UpdateThreadIterationDateTimeFinished(int threadIterationId)
        {
            lock (_threadJobLock)
            {
                var threadIteration = _threadIterations
                    .Single(x => x.ThreadIterationId == threadIterationId);

                threadIteration.DateTimeFinished = DateTime.UtcNow;
            }
        }
    }
}