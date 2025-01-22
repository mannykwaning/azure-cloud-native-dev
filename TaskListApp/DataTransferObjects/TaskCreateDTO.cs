using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TaskListApp.DataTransferObjects
{
    /// <summary>
    /// Task Creation Object
    /// </summary>
    public class TaskCreateDTO
    {
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

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"TaskName=[{TaskName}], IsCompleted=[{IsCompleted.ToString()}], DueDate=[{DueDate.ToString()}]";
        }
    }
}
