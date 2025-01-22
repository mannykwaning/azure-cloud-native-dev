using System.ComponentModel.DataAnnotations;
using TodoTasksApp.Models;

namespace TodoTasksApp.DataTransferObjects
{
    /// <summary>
    /// TodoTask Result Class Object
    /// </summary>
    public class TodoTaskResult
    {
        /// <summary>
        /// Initializes an instance of <see cref="TodoTaskResult"/> class using todotask as input.
        /// </summary>
        /// <param name="todotask"></param>
        public TodoTaskResult(TodoTask todotask)
        {
            Id = todotask.Id ?? -1;
            TaskName = todotask.TaskName;
            IsCompleted = todotask.IsCompleted;
            DueDate = todotask.DueDate.ToString();
        }

        /// <summary>
        /// Gets and Sets the Task's Id
        /// </summary>
        /// <value>
        /// Task Id
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets and sets the Task's Name
        /// </summary>
        /// <value>
        /// Task Name
        /// </value>
        [Required]
        [StringLength(30)]
        public string TaskName { get; set; }

        /// <summary>
        /// Gets and sets the Task's status
        /// </summary>
        /// <value>
        /// Completed true or false
        /// </value>
        [Required]
        public bool IsCompleted { get; set; }

        /// <summary>
        /// Gets and Sets the Task's Due Date
        /// </summary>
        [Required]
        public string DueDate { get; set; }
    }
}
