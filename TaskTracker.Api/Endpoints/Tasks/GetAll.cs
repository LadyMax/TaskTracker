using Microsoft.AspNetCore.Mvc;

namespace TaskTracker.Api.Endpoints.Task;
public class GetAllTasks : IEndpoint
{
    // Mapping
    public static void MapEndpoint(IEndpointRouteBuilder app) => app
        .MapGet("/tasks", Handle)
        .WithSummary("Gets all tasks");

    // Request and Response types
    public record Request(
       [FromQuery] Status? WithStatus = null,
       [FromQuery] DateTime? DueBefore = null,
       [FromQuery] OrderByField? OrderBy = null,
       [FromQuery] bool Descending = false
    );
    public record Response(
        int Id,
        string Title,
        string Description,
        Status Status,
        DateTime DueDate
    );

    //Logic
    private static List<Response> Handle([AsParameters] Request request, ITaskService taskService)
    {
        TaskItem[] tasks = Array.Empty<TaskItem>();

        if (request.WithStatus is not null || request.DueBefore is not null)
        {
            var filterCriteria = new FilterCriteria
            {
                WithStatus = request.WithStatus,
                DueBefore = request.DueBefore
            };
            tasks = taskService.GetFilteredTasks(filterCriteria);
        }
        else
        {
            tasks = taskService.GetAll();
        }

        if (request.OrderBy is not null)
        {
            var orderCriteria = new OrderCriteria
            {
                OrderBy = request.OrderBy,
                Descending = request.Descending
            };
            tasks = taskService.OrderTasks(tasks, orderCriteria);
        }

        return tasks.Select(task => new Response(
            task.Id,
            task.Title,
            task.Description,
            task.Status,
            task.DueDate
          )).ToList();
    }
}