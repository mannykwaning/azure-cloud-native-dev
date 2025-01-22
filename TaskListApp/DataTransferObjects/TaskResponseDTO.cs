using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using TaskListApp.Models;

namespace TaskListApp.DataTransferObjects
{
    public class TaskResponseDTO
    {
        public TaskResponseDTO(Models.TaskItem task)
        {
            Id = task.Id ?? -1;
            TaskName = task.TaskName;
            IsCompleted = task.IsCompleted;
            DueDate = task.DueDate.ToString();
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
