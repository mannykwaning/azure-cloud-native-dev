using System;
using System.Linq;
using TodoTasksApp.Models;

namespace TodoTasksApp.Data
{
    /// <summary>
    /// Initializes (seeds) the database with data
    /// </summary>
    /// <remarks>Step 7</remarks>
    public class DbInitializer
    {
        /// <summary>
        /// Initializes the specified context with data
        /// </summary>
        /// <param name="context">The context.</param>
        public static void Initialize(MyDatabaseContext context)
        {
            // Check to see if there is any data in the customer table
            if (context.TodoTasks.Any())
            {
                // Task table has data, nothing to do here
                return;
            }

            // Create some data
            TodoTask[] tasks = new TodoTask[]
            {
                new TodoTask()
                {
                    TaskName = "Do Laundry",
                    IsCompleted = false,
                    DueDate = new DateTime(2021, 02, 28)
                },
                new TodoTask()
                {
                    TaskName = "Workout",
                    IsCompleted = true,
                    DueDate = new DateTime(2021, 02, 27)
                },
                new TodoTask()
                {
                    TaskName = "Get wine",
                    IsCompleted = true,
                    DueDate = new DateTime(2021, 03, 01)
                },
                new TodoTask()
                {
                    TaskName = "Get groceries",
                    IsCompleted = false,
                    DueDate = new DateTime(2021, 03, 03)
                },
                new TodoTask()
                {
                    TaskName = "Wash car",
                    IsCompleted = false,
                    DueDate = new DateTime(2021, 02, 28)
                },
                 new TodoTask()
                {
                    TaskName = "Do homework",
                    IsCompleted = true,
                    DueDate = new DateTime(2021, 02, 27)
                }
            };

            // Add the data to the in memory model
            foreach (TodoTask t in tasks)
            {
                context.TodoTasks.Add(t);
            }

            // Commit the changes to the database
            context.SaveChanges();
        }
    }
}
