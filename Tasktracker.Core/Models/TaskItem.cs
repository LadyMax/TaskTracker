using System.ComponentModel;
using System.Text.Json.Serialization;

namespace TaskTracker.Core.Models;

public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public Status Status { get; set; }
    public DateTime DueDate { get; set; }

}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Status
{
    [Description("Task has not been started")]
    NotStarted,
    
    [Description("Task is currently in progress")]
    InProgress,
    
    [Description("Task has been completed")]
    Completed
}
