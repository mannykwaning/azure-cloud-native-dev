/*using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskListApp.Data;
using TaskListApp.Models;

namespace TaskListApp.Repository
{
    public class TasksRepository : ITasksRepository
    {
        private readonly TasksDatabaseContext _tasksDatabaseContext;
        private TasksRepository(TasksDatabaseContext context)
        {
            _tasksDatabaseContext = context;
        }
        public async Task<int> Add(TaskEntity task)
        {
            await _tasksDatabaseContext.Tasks.AddAsync(task);
            return 1;
        }

        public async Task<bool> Delete(int id)
        {
            var task = await _tasksDatabaseContext.Tasks.FindAsync(id);
            _tasksDatabaseContext.Remove(task);
            return true;
        }

        public async Task<TaskEntity> Get(int id)
        {
            return await _tasksDatabaseContext.Tasks.FindAsync(id);
        }

        public async Task<IEnumerable<TaskEntity>> GetAll()
        {
            return await _tasksDatabaseContext.Tasks.ToListAsync();
        }

        public async Task<bool> Update(TaskEntity task)
        {
            var taskItem = await _tasksDatabaseContext.Tasks.FindAsync(task);
            this._tasksDatabaseContext.Entry(taskItem).CurrentValues.SetValues(task);
            return true;
        }
    }
}
*/