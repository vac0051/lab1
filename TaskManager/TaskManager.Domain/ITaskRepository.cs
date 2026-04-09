using System;
using System.Collections.Generic;

namespace TaskManager.Domain
{
    public interface ITaskRepository
    {
        IEnumerable<TaskItem> GetAll();
        TaskItem? GetById(int id);
        void Add(TaskItem task);
        void Delete(int id);
        void Update(TaskItem task);
    }
}
