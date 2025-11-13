using Microsoft.AspNetCore.Http.HttpResults;

namespace TaskTracker.Api.Endpoints.Tasks;
public class CreateTask : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app) => app
        .MapPost("/tasks", Handle)
        .WithSummary("Create task");

    public record Request(
        string Title,
        string Description,
        Status Status,
        DateTime DueDate
        );
    public record Response(int id);

    private static Ok<Response> Handle(Request request, ITaskService taskService)
    {
        var newTask = new TaskItem { 
            Title = request.Title,
            Description = request.Description,
            Status = request.Status,
            DueDate = request.DueDate
        };
        newTask.Id = taskService.Create(newTask);

        return TypedResults.Ok(new Response(newTask.Id));
    }
}

