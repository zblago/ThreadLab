namespace ThreadLab.Database
{
    internal class IncrementerRepository
    {
        private static readonly object _threadJobLock = new object();
        private ThreadDbContext dbContext;

        public IncrementerRepository()
        {
            dbContext = new ThreadDbContext();
        }

        public int AddThreadJob(int managedThreadId, bool isBackground, int numberOfThreads, int numberOfStepsPerThread)
        {
            var threadJob = new ThreadJob
            {
                ManagedThreadId = managedThreadId,
                IsBackground = isBackground,
                NumberOfThreads = numberOfThreads,
                NumberOfStepsPerThread = numberOfStepsPerThread,
                DateTimeStarted = DateTime.UtcNow
            };

            
            {
                dbContext.ThreadJobs.Add(threadJob);
                dbContext.SaveChanges();
            }

            return threadJob.ThreadJobId;
        }

        public void UpdateThreadJobDateTimeFinished(int threadJobId)
        {
            dbContext.ThreadJobs.Find(threadJobId)!.DateTimeFinished = DateTime.UtcNow;
            dbContext.SaveChanges();
        }

        public bool HasDuplicateStartNumber(int threadJobId)
        {
            return dbContext.ThreadIterations
                .Where(x => x.ThreadJobId == threadJobId)
                .GroupBy(x => x.StartNumber)
                .Where(x => x.Count() > 1)
                .Any();
        }

        public bool HasDuplicateEndNumber(int threadJobId)
        {
            return dbContext.ThreadIterations
                .Where(x => x.ThreadJobId == threadJobId)
                .GroupBy(x => x.EndNumber)
                .Where(x => x.Count() > 1)
                .Any();
        }

        public bool HasGaps(int threadJobId)
        {
            ThreadJob threadJob;
            int lastIterationId;
            var pageSize = 1000;
            using (var context = new ThreadDbContext())
            {
                threadJob = context.ThreadJobs.Single(x => x.ThreadJobId == threadJobId);
                lastIterationId = context
                    .ThreadIterations
                    .Where(x => x.ThreadJobId == threadJobId)
                    .Max(x => x.ThreadIterationId);
            }
            for (var pageNo = 1; ; pageNo++)
            {
                var itemsToSkip = (pageNo - 1) * pageSize;
                var threadIterations = new List<ThreadIteration>();

                threadIterations = dbContext
                    .ThreadIterations
                    .Where(x => x.ThreadJobId == threadJobId)
                    .OrderBy(x => x.StartNumber)
                    .Skip(itemsToSkip)
                    .Take(pageSize)
                    .ToList();

                var i = 0;
                foreach (var iteration in threadIterations)
                {
                    var calculatedStartNumber = (i + itemsToSkip) * (long)threadJob.NumberOfStepsPerThread;
                    var isLastIteration = iteration.ThreadIterationId == lastIterationId;
                    var calculatedEndNumber = isLastIteration
                        ? long.MaxValue
                        : (calculatedStartNumber + threadJob.NumberOfStepsPerThread);

                    if (isLastIteration)
                        return false;

                    if (calculatedStartNumber != iteration.StartNumber || calculatedEndNumber != iteration.EndNumber)
                    {
                        return true;
                    }

                    ++i;
                }
            }
        }

        public int AddThreadIteration(int threadJobId, int managedThreadId, bool isBackground, long startNumber, long endNumber)
        {
            var threadIteration = new ThreadIteration
            {
                ThreadJobId = threadJobId,
                ManagedThreadId = managedThreadId,
                IsBackground = isBackground,
                StartNumber = startNumber,
                EndNumber = endNumber,
                DateTimeStarted = DateTime.UtcNow
            };

            lock (_threadJobLock)
            {
                dbContext.ThreadIterations.Add(threadIteration);
                dbContext.SaveChanges();
            }

            return threadIteration.ThreadIterationId;
        }

        public void UpdateThreadIterationDateTimeFinished(int threadIterationId)
        {
            lock (_threadJobLock)
            {
                dbContext.ThreadIterations.Find(threadIterationId)!.DateTimeFinished = DateTime.UtcNow;
                dbContext.SaveChanges();
            }
        }
    }
}
