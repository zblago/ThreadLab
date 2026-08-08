namespace ThreadLab.Database
{
    internal interface IIncrementerRepository
    {
        int AddThreadIteration(int threadJobId, int managedThreadId, bool isBackground, long startNumber, long endNumber);
        int AddThreadJob(int managedThreadId, bool isBackground, int numberOfThreads, int numberOfStepsPerThread);
        bool HasDuplicateEndNumber(int threadJobId);
        bool HasDuplicateStartNumber(int threadJobId);
        bool HasGaps(int threadJobId);
        void UpdateThreadIterationDateTimeFinished(int threadIterationId);
        void UpdateThreadJobDateTimeFinished(int threadJobId);
    }
}