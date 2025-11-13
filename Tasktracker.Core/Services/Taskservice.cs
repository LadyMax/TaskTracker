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
            // You might want to remove this when running unit tests
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

        public TaskItem[] GetFilteredTasks(FilterCriteria filterCriteria)
        {
            return _taskItems[1..5];
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
