using Microsoft.AspNetCore.Mvc;

namespace TaskTracker.Api.Endpoints.Task;
public class GetTask : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app) => app
        .MapGet("/tasks/{id}", Handle)
        .WithSummary("Gets a task by id");

    public record Request(int Id);

    public record Response(
        int Id,
        string Title,
        string Description,
        Status Status,
        DateTime DueDate
    );

    private static Response Handle([AsParameters] Request request, ITaskService taskService)
    {
        var task = taskService.GetById(request.Id);
        return new Response(
            task.Id,
            task.Title,
            task.Description,
            task.Status,
            task.DueDate
          );


    }
}