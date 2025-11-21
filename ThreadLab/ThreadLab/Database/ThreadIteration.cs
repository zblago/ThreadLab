using System.ComponentModel.DataAnnotations;

namespace ThreadLab.Database
{
    public class ThreadIteration
    {
        [Key]
        public int ThreadIterationId { get; set; }
        public int ThreadJobId { get; set; }
        public int ManagedThreadId { get; set; }
        public bool IsBackground { get; set; }
        public long StartNumber { get; set; }
        public long EndNumber { get; set; }
        public DateTime DateTimeStarted { get; set; }
        public DateTime? DateTimeFinished { get; set; }
    }
}
