using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskListApp.Models
{
    /// <summary>
    /// Task Entity
    /// </summary>
    public class TaskItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string TaskName { get; set; }
        
        [Required]
        public bool IsCompleted { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        public override string ToString()
        {
            return $"Id=[{Id}] TaskName=[{TaskName}], IsCompleted=[{IsCompleted}], DueDate=[{DueDate}]";
        }
    }
}
