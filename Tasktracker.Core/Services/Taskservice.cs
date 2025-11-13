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
            // Just loop through all tasks once and collect matches
            // O(n) time, O(n) space worst case
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
            // Insertion sort - for each element, find where it should go and shift others
            // O(n^2) worst case, but works well if data is almost sorted
            if (taskItems is null || taskItems.Length == 0)
            {
                return Array.Empty<TaskItem>();
            }

            var sorted = new TaskItem[taskItems.Length];
            for (int copyIndex = 0; copyIndex < taskItems.Length; copyIndex++)
            {
                sorted[copyIndex] = taskItems[copyIndex];
            }

            var orderBy = orderCriteria?.OrderBy ?? OrderByField.DueDate;
            var descending = orderCriteria?.Descending ?? false;

            for (int i = 1; i < sorted.Length; i++)
            {
                var current = sorted[i];
                var position = i - 1;

                while (position >= 0 && ShouldSwap(sorted[position], current, orderBy, descending))
                {
                    sorted[position + 1] = sorted[position];
                    position--;
                }

                sorted[position + 1] = current;
            }

            return sorted;
        }

        private static bool ShouldSwap(TaskItem? left, TaskItem? right, OrderByField field, bool descending)
        {
            if (right is null)
            {
                return false;
            }

            if (left is null)
            {
                return true;
            }

            var comparison = Compare(left, right, field);
            return descending ? comparison < 0 : comparison > 0;
        }

        private static int Compare(TaskItem left, TaskItem right, OrderByField field)
        {
            return field switch
            {
                OrderByField.DueDate => left.DueDate.CompareTo(right.DueDate),
                OrderByField.Title => CompareStrings(left.Title, right.Title),
                OrderByField.Status => ((int)left.Status).CompareTo((int)right.Status),
                _ => 0
            };
        }

        private static int CompareStrings(string? left, string? right)
        {
            // Handle null values - nulls go to the end
            if (left is null && right is null)
            {
                return 0;
            }

            if (left is null)
            {
                return 1;
            }

            if (right is null)
            {
                return -1;
            }

            return string.Compare(left, right, StringComparison.OrdinalIgnoreCase);
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
