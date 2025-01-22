using TaskListApp.Models;
using System.Linq;
using System;

namespace TaskListApp.Data
{
    /// <summary>
    /// Initializes (seeds) the database with data
    /// </summary>
    /// <remarks>Step 7</remarks>
    public class DBInitializer
    {
        /// <summary>
        /// Initializes the specified context with data
        /// </summary>
        /// <param name="context">The context.</param>
        public static void Initialize(TasksDatabaseContext context)
        {
            //context.Database.EnsureCreated();

            // Check to see if there is any data in the customer table
            if (context.TaskItems.Any())
            {
                // Task table has data, nothing to do here
                return;
            }

            // Create some data
            TaskItem[] tasks = new TaskItem[]
            {
                new TaskItem()
                {
                    TaskName = "Do Laundry",
                    IsCompleted = false,
                    DueDate = new DateTime(2021, 02, 28)
                },
                new TaskItem()
                {
                    TaskName = "Workout",
                    IsCompleted = true,
                    DueDate = new DateTime(2021, 02, 27)
                },
                new TaskItem()
                {
                    TaskName = "Get wine",
                    IsCompleted = true,
                    DueDate = new DateTime(2021, 03, 01)
                },
                new TaskItem()
                {
                    TaskName = "Get groceries",
                    IsCompleted = false,
                    DueDate = new DateTime(2021, 03, 03)
                },
                new TaskItem()
                {
                    TaskName = "Wash car",
                    IsCompleted = false,
                    DueDate = new DateTime(2021, 02, 28)
                },
                 new TaskItem()
                {
                    TaskName = "Do homework",
                    IsCompleted = true,
                    DueDate = new DateTime(2021, 02, 27)
                }
            };

            // Add the data to the in memory model
            foreach (TaskItem t in tasks)
            {
                context.TaskItems.Add(t);
            }

            // Commit the changes to the database
            context.SaveChanges();
        }
    }
}
