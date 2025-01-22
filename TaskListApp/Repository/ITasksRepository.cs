/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskListApp.Models;

namespace TaskListApp.Repository
{
    public interface ITasksRepository
    {
        Task<Task> Get(int id);
        Task<IEnumerable<Task>> GetAll();
        Task<int> Add(Models.Task task);
        Task<bool> Delete(int id);
        Task<bool> Update(Models.Task task);
    }
}
*/