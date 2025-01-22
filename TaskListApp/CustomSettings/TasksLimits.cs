using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskListApp.CustomSettings
{
    /// <summary>
    /// Tasks limits definition
    /// </summary>
    public class TasksLimits
    {
        public string ApplicationName { get; set; } = "Task Lists App";
        
        /// <summary>
        /// Gets and sets max number of tasks
        /// </summary>
        /// <value>
        /// Max number of customers
        /// </value>
        public int MaxTasks { get; set; } = 100;
    }
}
