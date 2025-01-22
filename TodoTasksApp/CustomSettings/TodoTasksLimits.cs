namespace TodoTasksApp.CustomSettings
{
    /// <summary>
    /// TodoTasks limits definition
    /// </summary>
    public class TodoTasksLimits
    {
        public string ApplicationName { get; set; } = "Task Lists App";

        /// <summary>
        /// Gets and sets max number of tasks
        /// </summary>
        /// <value>
        /// Max number of customers
        /// </value>
        public int MaxTasks { get; set; } = 10;
    }
}
