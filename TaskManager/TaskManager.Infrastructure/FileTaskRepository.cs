using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using TaskManager.Domain;

namespace TaskManager.Infrastructure
{
    public class FileTaskRepository : ITaskRepository
    {
        private readonly string _filePath;

        public FileTaskRepository(string filePath)
        {
            _filePath = filePath;
            if (!File.Exists(_filePath))
            {
                File.WriteAllText(_filePath, "[]");
            }
        }

        private List<TaskItem> LoadData()
        {
            if (!File.Exists(_filePath)) return new List<TaskItem>();
            var json = File.ReadAllText(_filePath);
            if (string.IsNullOrWhiteSpace(json)) return new List<TaskItem>();
            
            return JsonSerializer.Deserialize<List<TaskItem>>(json) ?? new List<TaskItem>();
        }

        private void SaveData(List<TaskItem> tasks)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(tasks, options);
            File.WriteAllText(_filePath, json);
        }

        public IEnumerable<TaskItem> GetAll()
        {
            return LoadData();
        }

        public TaskItem? GetById(int id)
        {
            var tasks = LoadData();
            return tasks.FirstOrDefault(t => t.Id == id);
        }

        public void Add(TaskItem task)
        {
            var tasks = LoadData();
            tasks.Add(task);
            SaveData(tasks);
        }

        public void Delete(int id)
        {
            var tasks = LoadData();
            var taskToRemove = tasks.FirstOrDefault(t => t.Id == id);
            if (taskToRemove != null)
            {
                tasks.Remove(taskToRemove);
                SaveData(tasks);
            }
        }

        public void Update(TaskItem task)
        {
            var tasks = LoadData();
            var index = tasks.FindIndex(t => t.Id == task.Id);
            if (index != -1)
            {
                tasks[index] = task;
                SaveData(tasks);
            }
        }
    }
}
