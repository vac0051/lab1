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

        public TaskItem AddTask(string title, string description)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("Task title cannot be empty.", nameof(title));
            }

            var tasks = _taskRepository.GetAll();
            var newId = tasks.Any() ? tasks.Max(t => t.Id) + 1 : 1;

            var task = new TaskItem
            {
                Id = newId,
                Title = title.Trim(),
                Description = description?.Trim() ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                IsCompleted = false
            };

            _taskRepository.Add(task);
            return task;
        }

        public bool DeleteTask(int id)
        {
            var task = _taskRepository.GetById(id);
            if (task is null)
            {
                return false;
            }

            _taskRepository.Delete(id);
            return true;
        }

        public bool CompleteTask(int id)
        {
            var task = _taskRepository.GetById(id);
            if (task is null)
            {
                return false;
            }

            task.IsCompleted = true;
            _taskRepository.Update(task);
            return true;
        }
    }
}
