using Microsoft.EntityFrameworkCore;
using TaskListApp.Models;

namespace TaskListApp.Data
{
    /// <summary>
    /// Use Entity Framework to Establish Database Interaction with Data Models
    /// </summary>
    /// <seealso cref="Microsoft.EntityFrameworkCore.DbContext" />
    public class TasksDatabaseContext : DbContext
    {
        /// <summary>
        /// Constructor for <see cref="TasksDatabaseContext"/> class.
        /// </summary>
        /// <param name="options"></param>
        public TasksDatabaseContext(DbContextOptions<TasksDatabaseContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        /// <summary>
        /// Tasks table (Entity Set)
        /// </summary>
        /// <value>
        /// Tasks
        /// </value>
        public DbSet<TaskItem> TaskItems { get; set; }

        /// <summary>
        /// For further configuration of discovered models
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskItem>().ToTable("TaskItem");
        }

    }
}
