using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using TodoTasksApp.Common;
using TodoTasksApp.CustomSettings;
using TodoTasksApp.Data;
using TodoTasksApp.DataTransferObjects;
using TodoTasksApp.ExtensionMethods;
using TodoTasksApp.Models;

namespace TodoTasksApp.Controllers
{
    /// <summary>
    /// REST Endpoints/ Implimentation for TodoTasks app 
    /// </summary>
    [Produces("application/json")]
    [ApiController]
    public class TodoTasksController : ControllerBase
    {
        private const string GetTaskByIdRoute = "GetTaskByIdRoute";

        private readonly MyDatabaseContext _myDatabaseContext;

        private readonly ILogger _logger;

        private readonly IConfiguration _configuration;

        private readonly TodoTasksLimits _tasksLimits;

        public TodoTasksController(ILogger<TodoTasksController> logger,
                              MyDatabaseContext context,
                              IConfiguration configuration,
                              IOptions<TodoTasksLimits> todotasksLimits)
        {
            _logger = logger;
            _myDatabaseContext = context;
            _configuration = configuration;
            _tasksLimits = todotasksLimits.Value;
        }

        /// <summary>
        /// Gets the requested task based on id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>A task</returns>
        [HttpGet]
        [ProducesResponseType(typeof(TodoTaskResult), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(void), 500)]
        [Route("api/v1/tasks/{id}", Name = GetTaskByIdRoute)]
        public IActionResult GetTaskById(int id)
        {
            try
            {
                Models.TodoTask todoTask = (from c in _myDatabaseContext.TodoTasks where c.Id == id orderby c.TaskName descending select c).SingleOrDefault();
                if (todoTask == null)
                {
                    _logger.LogInformation(LoggingEvents.GetItem, $"TodoTasksController TodoTask(id=[{id}]) was not found.", id);
                    return NotFound();
                }
                return new ObjectResult(new TodoTaskResult(todoTask));
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.InternalError, ex, $"TodoTasksController Get TodoTask(id=[{id}]) caused an internal error.", id);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }

        }

        /// <summary>
        /// Gets the list of tasks in the database
        /// </summary>
        /// <returns>The list of tasks</returns>
        [HttpGet]
        [ProducesResponseType(typeof(int[]), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(void), 500)]
        [Route("api/v1/tasks")]
        public IActionResult GetAllTodoTasks()
        {
            List<TodoTask> tasks = (from c in _myDatabaseContext.TodoTasks select c).ToList();

            return new ObjectResult(tasks);
        }

        /// <summary>
        /// Create a task NOTE: DATE FORMAT EX: "Friday, April 10, 2009" --> Date must be an actual calendar date
        /// Converts to  standard ISO 8601
        /// </summary>
        /// <param name="todoTaskCreatePayload"></param>
        /// <returns>An IAction result indicating HTTP 201 created if success otherwise BadRequest if the input is not valid</returns>
        [ProducesResponseType(typeof(TodoTaskResult), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(List<ErrorResponse>), StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(typeof(void), 500)]
        [Route("api/v1/tasks")]
        [HttpPost]
        public async Task<IActionResult> CreateAtodoTaskAsync([FromBody] TodoTaskCreatePayload todoTaskCreatePayload)
        {
            TodoTask todoTaskEntity = new TodoTask();

            try
            {
                if (ModelState.IsValid)
                {
                    // Check that the tasks limit has not been reached
                    if (!CanAddMoreTasks())
                    {
                        return StatusCode((int)HttpStatusCode.Forbidden, $"The maximum:[{_tasksLimits.MaxTasks}] number of Tasks have been created. No further tasks can be created at this time.");
                    }

                    var cultureInfo = new CultureInfo("en-US");
                    DateTime taskDate = DateTime.ParseExact(todoTaskCreatePayload.DueDate, "D", cultureInfo);

                    todoTaskEntity.TaskName = todoTaskCreatePayload.TaskName;
                    todoTaskEntity.IsCompleted = todoTaskCreatePayload.IsCompleted;
                    todoTaskEntity.DueDate = taskDate; // TODO: add validation to check date

                    // Add the task entity
                    _myDatabaseContext.TodoTasks.Add(todoTaskEntity);
                    // Save to DB
                    _myDatabaseContext.SaveChanges();
                }
                else
                {
                    List<ErrorResponse> errorResponses = new List<ErrorResponse>();

                    using StreamReader sr = new StreamReader(Request.Body);
                    Request.Body.Seek(0, SeekOrigin.Begin);
                    string inputJsonString = await sr.ReadToEndAsync();

                    using (JsonDocument jsonDocument = JsonDocument.Parse(inputJsonString))
                    {
                        // This is an approach for determining which properties have errors and knowing the
                        // property name as its the key value
                        foreach (string key in ModelState.Keys)
                        {
                            if (ModelState[key].ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid)
                            {
                                foreach (Microsoft.AspNetCore.Mvc.ModelBinding.ModelError error in ModelState[key].Errors)
                                {
                                    string cleansedKey = key.CleanseModelStateKey();
                                    string camelCaseKey = cleansedKey.ToCamelCase();

                                    System.Diagnostics.Trace.WriteLine($"MODEL ERROR: key:{cleansedKey} attemtedValue:{jsonDocument.RootElement.GetProperty(camelCaseKey)}, errorMessage:{error.ErrorMessage}");

                                    ErrorResponse errorResponse = new ErrorResponse();
                                    (errorResponse.ErrorDescription, errorResponse.ErrorNumber) = ErrorResponse.GetErrorMessage(error.ErrorMessage);
                                    errorResponse.ParameterName = camelCaseKey;
                                    errorResponse.ParameterValue = jsonDocument.RootElement.GetProperty(camelCaseKey).ToString();
                                    errorResponses.Add(errorResponse);
                                }
                            }
                        }
                    }

                    return BadRequest(errorResponses);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.InternalError, ex, $"TodoTasksController Post todoTaskEntity([{todoTaskEntity}]) todoTaskCreatePayload([{todoTaskCreatePayload}] caused an internal error.");
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }

            return CreatedAtRoute(GetTaskByIdRoute, new { id = todoTaskEntity.Id }, new TodoTaskResult(todoTaskEntity));
        }

        /// <summary>
        /// Updates or Creates a task NOTE: DATE FORMAT EX: "Friday, April 10, 2009" --> Date must be an actual calendar date
        /// </summary>
        /// <param name="id">The identifier of the task.</param>
        /// <param name="taskUpdatePayload">The task.</param>
        /// <returns>An IAction result indicating HTTP 204 no content if success update
        /// HTTP 201 if successful create
        /// otherwise BadRequest if the input is not valid.</returns>
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(TodoTaskResult), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(List<ErrorResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), 500)]
        [Route("api/v1/tasks/{id}")]
        [HttpPut]
        public async Task<IActionResult> UpdateOrCreateTaskAsync(int id, [FromBody] TodoTaskUpdatePayload taskUpdatePayload)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    TodoTask todoTaskEntity = (from c in _myDatabaseContext.TodoTasks where c.Id == id select c).SingleOrDefault();

                    var cultureInfo = new CultureInfo("en-US");
                    DateTime taskDate = DateTime.ParseExact(taskUpdatePayload.DueDate, "D", cultureInfo);

                    // If the task doesn't exist in the database then create a new one
                    if (todoTaskEntity == null)
                    {
                        using (IDbContextTransaction transaction = _myDatabaseContext.Database.BeginTransaction())
                        {
                            // Verify that the max limit of tasks hasn't been reached
                            if (!CanAddMoreTasks())
                            {
                                return StatusCode((int)HttpStatusCode.Forbidden, $"Task limit reached MaxTasks: [{_tasksLimits.MaxTasks}]");
                            }

                            todoTaskEntity = new TodoTask()
                            {
                                Id = id,
                                TaskName = taskUpdatePayload.TaskName,
                                DueDate = taskDate,
                                IsCompleted = taskUpdatePayload.IsCompleted
                            };

                            // Turn identity insert on so that we can set the ID provided by the client
                            _myDatabaseContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT dbo.TodoTask ON;");
                            _myDatabaseContext.TodoTasks.Add(todoTaskEntity);
                            _myDatabaseContext.SaveChanges();

                            // Turn identity insert off so the default behavior of new identiy by SQL is used.
                            _myDatabaseContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT dbo.TodoTask OFF");
                            transaction.Commit();

                            return CreatedAtRoute(GetTaskByIdRoute, new { id = todoTaskEntity.Id }, new TodoTaskResult(todoTaskEntity));
                        }
                    }

                    // Update the task as requested
                    todoTaskEntity.TaskName = taskUpdatePayload.TaskName;
                    todoTaskEntity.IsCompleted = taskUpdatePayload.IsCompleted;
                    todoTaskEntity.DueDate = taskDate;

                    _myDatabaseContext.SaveChanges();
                }
                else
                {
                    List<ErrorResponse> errorResponses = new List<ErrorResponse>();

                    using StreamReader sr = new StreamReader(Request.Body);
                    Request.Body.Seek(0, SeekOrigin.Begin);
                    string inputJsonString = await sr.ReadToEndAsync();

                    using (JsonDocument jsonDocument = JsonDocument.Parse(inputJsonString))
                    {
                        // This is an approach for determining which properties have errors and knowing the
                        // property name as its the key value
                        foreach (string key in ModelState.Keys)
                        {
                            if (ModelState[key].ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid)
                            {
                                foreach (Microsoft.AspNetCore.Mvc.ModelBinding.ModelError error in ModelState[key].Errors)
                                {
                                    string cleansedKey = key.CleanseModelStateKey();
                                    string camelCaseKey = cleansedKey.ToCamelCase();

                                    System.Diagnostics.Trace.WriteLine($"MODEL ERROR: key:{cleansedKey} attemtedValue:{jsonDocument.RootElement.GetProperty(camelCaseKey)}, errorMessage:{error.ErrorMessage}");

                                    ErrorResponse errorResponse = new ErrorResponse();
                                    (errorResponse.ErrorDescription, errorResponse.ErrorNumber) = ErrorResponse.GetErrorMessage(error.ErrorMessage);
                                    errorResponse.ParameterName = camelCaseKey;
                                    errorResponse.ParameterValue = jsonDocument.RootElement.GetProperty(camelCaseKey).ToString();
                                    errorResponses.Add(errorResponse);
                                }
                            }
                        }
                    }

                    return BadRequest(errorResponses);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.InternalError, ex, $"TodoTasksController Put task(id=[{id}]) caused an internal error.", id);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }

            return NoContent();
        }

        /// <summary>
        /// Deletes a task using the provided id
        /// </summary>
        /// <param name="id">The identifier of the task.</param>        
        /// <returns>An IAction result indicating HTTP 204 no content if success otherwise BadRequest if the input is not valid.</returns>
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(void), 500)]
        [Route("api/v1/tasks/{id}")]
        [HttpDelete]
        public IActionResult DeleteTaskById(int id)
        {
            try
            {
                TodoTask dbTask = (from c in _myDatabaseContext.TodoTasks where c.Id == id select c).SingleOrDefault();

                if (dbTask == null)
                {
                    _logger.LogInformation(LoggingEvents.UpdateItem, $"TodoTasksController task(id=[{id}]) was not found.", id);
                    return NotFound();
                }

                _myDatabaseContext.TodoTasks.Remove(dbTask);
                _myDatabaseContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.InternalError, ex, $"TodoTasksController delete task(id=[{id}]) caused an internal error.", id);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }

            return NoContent();
        }

        /// <summary>
        /// Determines if the tasks limit has been reached
        /// </summary>
        /// <returns>True or False if a task can be added or not</returns>
        private bool CanAddMoreTasks()
        {
            long totalTasks = (from c in _myDatabaseContext.TodoTasks select c).Count();

            return _tasksLimits.MaxTasks > totalTasks;
        }
    }
}
