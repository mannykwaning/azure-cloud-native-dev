using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Threading.Tasks;
using TaskListApp.Common;
using TaskListApp.CustomSettings;
using TaskListApp.Data;
using TaskListApp.DataTransferObjects;
using TaskListApp.Models;
using System.Linq;

namespace TaskListApp.Controllers
{
    /// <summary>
    /// REST Endpoints/ Implimentation for Tasks List app 
    /// </summary>
    [Produces("application/json")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private const string GetTaskByIdRoute = "GetTaskByIdRoute";

        private readonly TasksDatabaseContext _tasksDatabaseContext;

        private readonly ILogger _logger;

        private readonly IConfiguration _configuration;

        private readonly TasksLimits _tasksLimits;

        //private ITasksRepository _tasksRepository;

        public TasksController(ILogger<TasksController> logger,
                               TasksDatabaseContext context,
                               IConfiguration configuration,
                               IOptions<TasksLimits> tasksLimits
                               /*ITasksRepository tasksRepository*/)
        {
            _logger = logger;
            _tasksDatabaseContext = context;
            _configuration = configuration;
            _tasksLimits = tasksLimits.Value;
            //_tasksRepository = tasksRepository;
        }

        /// <summary>
        /// Gets the requested task based on id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>A task</returns>
        [HttpGet]
        [ProducesResponseType(typeof(TaskResponseDTO), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(void), 500)]
        [Route("api/v1/tasks/{id}", Name = GetTaskByIdRoute)]
        public IActionResult GetTaskByIdAsync(int id)
        {
            try
            {
                Models.TaskItem task = (from c in _tasksDatabaseContext.TaskItems where c.Id == id orderby c.TaskName descending select c).SingleOrDefault();
                if (task == null)
                {
                    _logger.LogInformation(LoggingEvents.GetItem, $"TasksController Task(id=[{id}]) was not found.", id);
                    return NotFound();
                }
                return new ObjectResult(new TaskResponseDTO(task));
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.InternalError, ex, $"TasksController Get Task(id=[{id}]) caused an internal error.", id);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }

        }

    }
}
