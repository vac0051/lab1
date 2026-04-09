using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskManager.Application;
using TaskManager.Domain;
using TaskManager.Infrastructure;

namespace TaskManager.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            var taskService = host.Services.GetRequiredService<ITaskService>();

            logger.LogInformation("Приложение запущено.");

            RunLoop(taskService, logger);

            logger.LogInformation("Приложение завершило работу.");
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(AppContext.BaseDirectory);
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    var dataFilePath = context.Configuration["DataFilePath"] ?? "tasks.json";

                    services.AddSingleton<ITaskRepository>(provider => 
                        new FileTaskRepository(dataFilePath));
                    
                    services.AddTransient<ITaskService, TaskService>();
                });

        static void RunLoop(ITaskService taskService, ILogger logger)
        {
            while (true)
            {
                Console.WriteLine("\n=== Менеджер Задач ===");
                Console.WriteLine("1. Добавить задачу");
                Console.WriteLine("2. Удалить задачу");
                Console.WriteLine("3. Просмотреть все задачи");
                Console.WriteLine("4. Завершить задачу");
                Console.WriteLine("5. Выход");
                Console.Write("Выберите действие: ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.Write("Введите название задачи: ");
                        var title = Console.ReadLine();
                        Console.Write("Введите описание задачи: ");
                        var description = Console.ReadLine() ?? string.Empty;
                        if (string.IsNullOrWhiteSpace(title))
                        {
                            Console.WriteLine("Название не может быть пустым!");
                            logger.LogWarning("Попытка добавить задачу с пустым названием.");
                        }
                        else
                        {
                            taskService.AddTask(title, description);
                            Console.WriteLine("Задача успешно добавлена!");
                            logger.LogInformation("Добавлена новая задача: {Title}", title);
                        }
                        break;
                    case "2":
                        PrintTasks(taskService);
                        Console.Write("Введите ID задачи для удаления: ");
                        if (int.TryParse(Console.ReadLine(), out int deleteId))
                        {
                            taskService.DeleteTask(deleteId);
                            Console.WriteLine("Задача удалена (если существовала).");
                            logger.LogInformation("Запрошено удаление задачи с ID: {Id}", deleteId);
                        }
                        else
                        {
                            Console.WriteLine("Неверный формат ID.");
                            logger.LogWarning("Пользователь ввел некорректный ID для удаления.");
                        }
                        break;
                    case "3":
                        PrintTasks(taskService);
                        logger.LogInformation("Пользователь просмотрел список задач.");
                        break;
                    case "4":
                        PrintTasks(taskService);
                        Console.Write("Введите ID задачи для завершения: ");
                        if (int.TryParse(Console.ReadLine(), out int completeId))
                        {
                            taskService.CompleteTask(completeId);
                            Console.WriteLine("Задача помечена как завершенная (если существовала).");
                            logger.LogInformation("Запрошено завершение задачи с ID: {Id}", completeId);
                        }
                        else
                        {
                            Console.WriteLine("Неверный формат ID.");
                            logger.LogWarning("Пользователь ввел некорректный ID для завершения.");
                        }
                        break;
                    case "5":
                        return;
                    default:
                        Console.WriteLine("Неизвестная команда.");
                        break;
                }
            }
        }

        static void PrintTasks(ITaskService taskService)
        {
            var tasks = taskService.GetAllTasks();
            Console.WriteLine("\n--- Список задач ---");
            bool hasTasks = false;
            foreach (var task in tasks)
            {
                hasTasks = true;
                string status = task.IsCompleted ? "[Завершена]" : "[Активна]";
                Console.WriteLine($"{task.Id} | {status} {task.Title} - {task.Description} (Создано: {task.CreatedAt})");
            }
            if (!hasTasks) Console.WriteLine("Список задач пуст.");
            Console.WriteLine("--------------------");
        }
    }
}
