using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ThreadLab.Database
{
    internal class ThreadDbContext : DbContext
    {
        public DbSet<ThreadJob> ThreadJobs { get; set; }
        public DbSet<ThreadIteration> ThreadIterations { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Load configuration from appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var useInMemory = configuration.GetValue<bool>("UseInMemoryDatabase");
            if (useInMemory)
            {
                optionsBuilder.UseInMemoryDatabase("ThreadLabDb");
            }
            else
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ThreadJob>()
                .Property(u => u.ThreadJobId)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<ThreadJob>()
                .HasMany<ThreadIteration>(x => x.ThreadIterations);

            modelBuilder.Entity<ThreadIteration>()
                .Property(u => u.ThreadIterationId)
                .ValueGeneratedOnAdd();
        }
    }
}
