using TaskTracker.Core.Models;

namespace TaskTracker.Core.Interfaces;

public interface ITaskService
{
    TaskItem[] GetAll();
    TaskItem[] GetFilteredTasks(FilterCriteria filterCriteria);
    TaskItem[] OrderTasks(TaskItem[] tasks, OrderCriteria orderCriteria);
    TaskItem GetById(int id);
    TaskItem GetMostUrgent();
    int Create(TaskItem taskItem);

}
