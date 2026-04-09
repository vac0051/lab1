using System;
using System.Collections.Generic;
using System.Linq;
using TaskManager.Domain;

namespace TaskManager.Application
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;

        public TaskService(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public IEnumerable<TaskItem> GetAllTasks()
        {
            return _taskRepository.GetAll();
        }

        public void AddTask(string title, string description)
        {
            var tasks = _taskRepository.GetAll();
            var newId = tasks.Any() ? tasks.Max(t => t.Id) + 1 : 1;

            var task = new TaskItem
            {
                Id = newId,
                Title = title,
                Description = description,
                CreatedAt = DateTime.UtcNow,
                IsCompleted = false
            };
            _taskRepository.Add(task);
        }

        public void DeleteTask(int id)
        {
            _taskRepository.Delete(id);
        }

        public void CompleteTask(int id)
        {
            var task = _taskRepository.GetById(id);
            if (task != null)
            {
                task.IsCompleted = true;
                _taskRepository.Update(task);
            }
        }
    }
}
