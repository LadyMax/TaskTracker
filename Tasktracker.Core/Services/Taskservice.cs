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
                Array.Copy(_taskItems, newTaskItems, _taskItems.Length);
                _taskItems = newTaskItems;
            }
            taskItem.Id = _nextId;
            _taskItems[_nextId] = taskItem;
            _nextId++;

            return taskItem.Id;
        }

        public TaskItem GetMostUrgent()
        {
            // Find task with highest urgency score by checking all tasks
            // O(n) time
            if (_nextId == 0)
            {
                throw new InvalidOperationException("No tasks available to evaluate urgency.");
            }

            var referenceTime = DateTime.UtcNow;
            TaskItem? mostUrgent = null;
            var highestScore = double.NegativeInfinity;

            for (int i = 0; i < _nextId; i++)
            {
                var task = _taskItems[i];
                if (task is null)
                {
                    continue;
                }

                var score = CalculateUrgencyScore(task, referenceTime);
                if (mostUrgent is null || score > highestScore)
                {
                    highestScore = score;
                    mostUrgent = task;
                }
            }

            return mostUrgent ?? throw new InvalidOperationException("No tasks available to evaluate urgency.");
        }

        public TaskItem GetById(int id)
        {
            // Linear search for task with matching id
            // O(n) time
            for (int i = 0; i < _nextId; i++)
            {
                var task = _taskItems[i];
                if (task?.Id == id)
                {
                    return task;
                }
            }

            throw new KeyNotFoundException($"Task with id {id} was not found.");
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
                Array.Copy(_taskItems, copy, totalTasks);
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
            Array.Copy(buffer, result, matches);
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
            Array.Copy(taskItems, sorted, taskItems.Length);

            var orderBy = orderCriteria?.OrderBy ?? OrderByField.DueDate;
            var descending = orderCriteria?.Descending ?? false;
            var referenceTime = DateTime.UtcNow;

            if (orderBy == OrderByField.Status)
            {
                return CountingSortByStatus(sorted, descending);
            }

            for (int i = 1; i < sorted.Length; i++)
            {
                var current = sorted[i];
                var position = i - 1;

                while (position >= 0 && ShouldSwap(sorted[position], current, orderBy, descending, referenceTime))
                {
                    sorted[position + 1] = sorted[position];
                    position--;
                }

                sorted[position + 1] = current;
            }

            return sorted;
        }

        private static bool ShouldSwap(TaskItem? left, TaskItem? right, OrderByField field, bool descending, DateTime referenceTime)
        {
            if (right is null)
            {
                return false;
            }

            if (left is null)
            {
                return true;
            }

            var comparison = Compare(left, right, field, referenceTime);
            return descending ? comparison < 0 : comparison > 0;
        }

        private static int Compare(TaskItem left, TaskItem right, OrderByField field, DateTime referenceTime)
        {
            // Compare two tasks based on the specified field
            // O(1) time for each comparison
            return field switch
            {
                OrderByField.DueDate => left.DueDate.CompareTo(right.DueDate),
                OrderByField.Title => CompareStrings(left.Title, right.Title),
                OrderByField.Status => ((int)left.Status).CompareTo((int)right.Status),
                OrderByField.Urgency => CompareByUrgency(left, right, referenceTime),
                _ => 0
            };
        }

        private static int CompareByUrgency(TaskItem left, TaskItem right, DateTime referenceTime)
        {
            // Calculate urgency scores once to avoid duplicate calculations
            var leftScore = CalculateUrgencyScore(left, referenceTime);
            var rightScore = CalculateUrgencyScore(right, referenceTime);
            return leftScore.CompareTo(rightScore);
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

        private static TaskItem[] CountingSortByStatus(TaskItem[] source, bool descending)
        {
            // Counting sort works well when we have limited distinct values
            // O(n + k) where k is number of status types (small constant)
            var statusValues = Enum.GetValues<Status>();
            var statusCount = statusValues.Length;
            var counts = new int[statusCount];

            for (int i = 0; i < source.Length; i++)
            {
                var task = source[i];
                if (task is null)
                {
                    continue;
                }

                var bucket = GetStatusOrderingIndex(task.Status, descending, statusCount);
                counts[bucket]++;
            }

            for (int i = 1; i < counts.Length; i++)
            {
                counts[i] += counts[i - 1];
            }

            var output = new TaskItem[source.Length];
            var nullInsertIndex = source.Length - 1;
            for (int i = source.Length - 1; i >= 0; i--)
            {
                var task = source[i];
                if (task is null)
                {
                    output[nullInsertIndex] = null!;
                    nullInsertIndex--;
                    continue;
                }

                var bucket = GetStatusOrderingIndex(task.Status, descending, statusCount);
                counts[bucket]--;
                output[counts[bucket]] = task;
            }

            return output;
        }

        private static int GetStatusOrderingIndex(Status status, bool descending, int totalStatuses)
        {
            var index = (int)status;
            if (!descending)
            {
                return index;
            }

            return (totalStatuses - 1) - index;
        }

        private static double CalculateUrgencyScore(TaskItem task, DateTime referenceTime)
        {
            // Closer deadline = higher urgency, unfinished tasks get bonus
            var minutesToDue = (task.DueDate - referenceTime).TotalMinutes;
            var baseScore = -minutesToDue;
            var statusAdjustment = task.Status switch
            {
                Status.NotStarted => 60.0,
                Status.InProgress => 30.0,
                Status.Completed => -240.0,
                _ => 0.0
            };

            return baseScore + statusAdjustment;
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
