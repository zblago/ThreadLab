using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ThreadLab.Database
{
    internal class ThreadDbContext : DbContext
    {
        public ThreadDbContext()
        { 
        }

        public DbSet<ThreadJob> ThreadJobs { get; set; }
        public DbSet<ThreadIteration> ThreadIterations { get; set; }

        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Load configuration from appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var storageType = Enum.Parse<StorageType>(configuration.GetValue<string>("StorageType"));
            if (storageType == StorageType.InMemoryEfDbContext)
            {
                optionsBuilder.UseInMemoryDatabase("ThreadLab");
            }
            else if (storageType == StorageType.SQLServerEfDbContext)
            {
                var connectionString = configuration.GetConnectionString("ThreadLab");
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
