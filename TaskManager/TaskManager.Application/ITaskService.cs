using System.Collections.Generic;
using TaskManager.Domain;

namespace TaskManager.Application
{
    public interface ITaskService
    {
        IEnumerable<TaskItem> GetAllTasks();
        TaskItem AddTask(string title, string description);
        bool DeleteTask(int id);
        bool CompleteTask(int id);
    }
}
