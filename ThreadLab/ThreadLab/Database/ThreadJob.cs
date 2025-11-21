using System.ComponentModel.DataAnnotations;

namespace ThreadLab.Database
{
    public class ThreadJob
    {
        [Key]
        public int ThreadJobId { get; set; }
        public int ManagedThreadId { get; set; }
        public bool IsBackground { get; set; }
        public int NumberOfThreads { get; set; }
        public int NumberOfStepsPerThread { get; set; }
        public DateTime DateTimeStarted { get; set; }
        public DateTime? DateTimeFinished { get; set; }

        public ICollection<ThreadIteration> ThreadIterations { get; set; }
    }
}
