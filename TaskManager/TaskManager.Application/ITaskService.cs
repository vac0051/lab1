using System;
using System.Collections.Generic;
using TaskManager.Domain;

namespace TaskManager.Application
{
    public interface ITaskService
    {
        IEnumerable<TaskItem> GetAllTasks();
        void AddTask(string title, string description);
        void DeleteTask(int id);
        void CompleteTask(int id);
    }
}
