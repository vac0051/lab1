using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskManager.Application;
using TaskManager.Domain;
using TaskManager.Infrastructure;

namespace TaskManager.ConsoleApp;

internal static class Program
{
    private static void Main(string[] args)
    {
        Console.InputEncoding = Encoding.UTF8;
        Console.OutputEncoding = Encoding.UTF8;

        using var host = CreateHostBuilder(args).Build();
        var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("ConsoleUI");
        var taskService = host.Services.GetRequiredService<ITaskService>();

        logger.LogInformation("Application started.");

        try
        {
            RunLoop(taskService, logger);
        }
        catch (Exception exception)
        {
            logger.LogCritical(exception, "Unexpected fatal error.");
            Console.WriteLine("Произошла критическая ошибка. Подробности записаны в лог.");
        }
        finally
        {
            logger.LogInformation("Application stopped.");
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((_, config) =>
            {
                config.SetBasePath(AppContext.BaseDirectory);
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                var dataFilePath = context.Configuration["DataFilePath"] ?? "tasks.json";

                services.AddSingleton<ITaskRepository>(_ => new FileTaskRepository(dataFilePath));
                services.AddTransient<ITaskService, TaskService>();
            });

    private static void RunLoop(ITaskService taskService, ILogger logger)
    {
        while (true)
        {
            PrintMenu();
            Console.Write("Выберите действие: ");
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    AddTaskFlow(taskService, logger);
                    break;
                case "2":
                    DeleteTaskFlow(taskService, logger);
                    break;
                case "3":
                    PrintTasks(taskService);
                    logger.LogInformation("Task list opened by user.");
                    break;
                case "4":
                    CompleteTaskFlow(taskService, logger);
                    break;
                case "5":
                    return;
                default:
                    Console.WriteLine("Неизвестная команда. Повторите ввод.");
                    logger.LogWarning("Unknown menu command: {Choice}", choice);
                    break;
            }
        }
    }

    private static void PrintMenu()
    {
        Console.WriteLine();
        Console.WriteLine("=== Менеджер задач ===");
        Console.WriteLine("1. Добавить задачу");
        Console.WriteLine("2. Удалить задачу");
        Console.WriteLine("3. Показать все задачи");
        Console.WriteLine("4. Завершить задачу");
        Console.WriteLine("5. Выход");
    }

    private static void AddTaskFlow(ITaskService taskService, ILogger logger)
    {
        Console.Write("Введите название задачи: ");
        var title = Console.ReadLine();
        Console.Write("Введите описание задачи: ");
        var description = Console.ReadLine() ?? string.Empty;

        try
        {
            var task = taskService.AddTask(title ?? string.Empty, description);
            Console.WriteLine($"Задача добавлена. ID: {task.Id}");
            logger.LogInformation("Task created. Id: {TaskId}, Title: {Title}", task.Id, task.Title);
        }
        catch (ArgumentException)
        {
            Console.WriteLine("Название задачи не может быть пустым.");
            logger.LogWarning("Create task rejected because title is empty.");
        }
    }

    private static void DeleteTaskFlow(ITaskService taskService, ILogger logger)
    {
        PrintTasks(taskService);
        Console.Write("Введите ID задачи для удаления: ");

        if (!int.TryParse(Console.ReadLine(), out var deleteId))
        {
            Console.WriteLine("ID должен быть числом.");
            logger.LogWarning("Invalid task id in delete flow.");
            return;
        }

        var deleted = taskService.DeleteTask(deleteId);
        if (deleted)
        {
            Console.WriteLine("Задача удалена.");
            logger.LogInformation("Task deleted. Id: {TaskId}", deleteId);
        }
        else
        {
            Console.WriteLine("Задача с таким ID не найдена.");
            logger.LogWarning("Delete skipped. Task not found: {TaskId}", deleteId);
        }
    }

    private static void CompleteTaskFlow(ITaskService taskService, ILogger logger)
    {
        PrintTasks(taskService);
        Console.Write("Введите ID задачи для завершения: ");

        if (!int.TryParse(Console.ReadLine(), out var completeId))
        {
            Console.WriteLine("ID должен быть числом.");
            logger.LogWarning("Invalid task id in complete flow.");
            return;
        }

        var completed = taskService.CompleteTask(completeId);
        if (completed)
        {
            Console.WriteLine("Задача отмечена как завершенная.");
            logger.LogInformation("Task completed. Id: {TaskId}", completeId);
        }
        else
        {
            Console.WriteLine("Задача с таким ID не найдена.");
            logger.LogWarning("Complete skipped. Task not found: {TaskId}", completeId);
        }
    }

    private static void PrintTasks(ITaskService taskService)
    {
        var tasks = taskService.GetAllTasks().ToList();

        Console.WriteLine();
        Console.WriteLine("--- Список задач ---");

        if (tasks.Count == 0)
        {
            Console.WriteLine("Список задач пуст.");
            Console.WriteLine("--------------------");
            return;
        }

        foreach (var task in tasks)
        {
            var status = task.IsCompleted ? "Завершена" : "Активна";
            Console.WriteLine($"{task.Id} | [{status}] {task.Title} - {task.Description} (Создано: {task.CreatedAt:dd.MM.yyyy HH:mm})");
        }

        Console.WriteLine("--------------------");
    }
}
