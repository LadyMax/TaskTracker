using TaskTracker.Core.Interfaces;
using TaskTracker.Core.Models;

namespace TaskTracker.Core.Services
{

    public class TaskService : ITaskService
    {
        TaskItem[] _taskItems;
        int _nextId = 0;

        public TaskService()
        {
            // TODO: Remove this when the database is implemented
            _taskItems = GenerateDemoTaskItems();
            _nextId = _taskItems.Length;
        }

        public int Create(TaskItem taskItem)
        {
            if(_nextId >= _taskItems.Length)
            {
                var newTaskItems = new TaskItem[_taskItems.Length * 2];
                for (int i = 0; i < _taskItems.Length; i++)
                {
                    newTaskItems[i] = _taskItems[i];
                }
                _taskItems = newTaskItems;
            }
            taskItem.Id = _nextId;
            _taskItems[_nextId] = taskItem;
            _nextId++;

            return taskItem.Id;
        }

        public TaskItem GetMostUrgent()
        {
            return _taskItems[1];
        }

        public TaskItem GetById(int id)
        {
            return _taskItems[0];
        }

        public TaskItem[] GetAll()
        {
            return _taskItems[0.._nextId];
        }

        public TaskItem[] GetFilteredTasks(FilterCriteria? filterCriteria)
        {
            var totalTasks = _nextId;
            if (totalTasks == 0)
            {
                return Array.Empty<TaskItem>();
            }

            var hasCriteria = filterCriteria is not null &&
                              (filterCriteria.WithStatus.HasValue || filterCriteria.DueBefore.HasValue);
            if (!hasCriteria)
            {
                var copy = new TaskItem[totalTasks];
                for (int i = 0; i < totalTasks; i++)
                {
                    copy[i] = _taskItems[i];
                }

                return copy;
            }

            var buffer = new TaskItem[totalTasks];
            var matches = 0;
            var shouldFilterStatus = filterCriteria!.WithStatus.HasValue;
            var shouldFilterDue = filterCriteria.DueBefore.HasValue;
            var statusToMatch = filterCriteria.WithStatus.GetValueOrDefault();
            var dueBefore = filterCriteria.DueBefore.GetValueOrDefault();

            for (int i = 0; i < totalTasks; i++)
            {
                var task = _taskItems[i];
                if (task is null)
                {
                    continue;
                }

                if (shouldFilterStatus && task.Status != statusToMatch)
                {
                    continue;
                }

                if (shouldFilterDue && task.DueDate >= dueBefore)
                {
                    continue;
                }

                buffer[matches] = task;
                matches++;
            }

            if (matches == 0)
            {
                return Array.Empty<TaskItem>();
            }

            var result = new TaskItem[matches];
            for (int i = 0; i < matches; i++)
            {
                result[i] = buffer[i];
            }

            return result;
        }

        public TaskItem[] OrderTasks(TaskItem[] taskItems, OrderCriteria orderCriteria)
        {
            return taskItems[1.._nextId].Reverse().ToArray();
        }


        private TaskItem[] GenerateDemoTaskItems()
        {
            var items = new TaskItem[10];
            for (int i = 0; i < 10; i++)
            {
                items[i] = new TaskItem
                {
                    Id = i,
                    Title = $"Task {i}",
                    Description = $"Description {i}",
                    DueDate = DateTime.Now.AddDays(i),
                    Status = Status.NotStarted,
                };
            }
            return items;
        }
    }
}
