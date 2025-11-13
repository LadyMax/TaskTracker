using TaskTracker.Core.Models;
using TaskTracker.Core.Services;

namespace TaskTracker.Test
{
    public class TaskServiceTests
    {
        [Fact]
        public void CanGetAllTasks()
        {
            // Arrange
            var sut = new TaskService();

            // Act
            var items = sut.GetAll();

            // Assert
            Assert.NotNull(items);
        }

        [Fact]
        public void GetById_ReturnsCorrectTask()
        {
            // Arrange
            var sut = new TaskService();
            var all = sut.GetAll();
            var target = all[3];

            // Act
            var result = sut.GetById(target.Id);

            // Assert
            Assert.Equal(target.Id, result.Id);
        }

        [Fact]
        public void GetMostUrgent_ReturnsTaskWithHighestUrgency()
        {
            // Arrange
            var sut = new TaskService();
            var tasks = sut.GetAll();
            var now = DateTime.UtcNow;
            tasks[0].DueDate = now.AddHours(5);
            tasks[0].Status = Status.NotStarted;
            tasks[1].DueDate = now.AddMinutes(30);
            tasks[1].Status = Status.InProgress;
            tasks[2].DueDate = now.AddMinutes(5);
            tasks[2].Status = Status.NotStarted;
            tasks[3].DueDate = now.AddHours(-1);
            tasks[3].Status = Status.Completed;

            // Act
            var result = sut.GetMostUrgent();

            // Assert
            Assert.Equal(tasks[2].Id, result.Id);
        }

        [Fact]
        public void FilterTasks_ByStatus_ReturnsOnlyMatchingStatus()
        {
            // Arrange
            var sut = new TaskService();
            var tasks = sut.GetAll();
            tasks[0].Status = Status.Completed;
            tasks[1].Status = Status.Completed;
            tasks[2].Status = Status.InProgress;

            var criteria = new FilterCriteria
            {
                WithStatus = Status.Completed
            };

            // Act
            var result = sut.GetFilteredTasks(criteria);

            // Assert
            Assert.All(result, task => Assert.Equal(Status.Completed, task.Status));
            Assert.Equal(2, result.Length);
        }

        [Fact]
        public void FilterTasks_ByDueDate_ReturnsOnlyEarlierTasks()
        {
            // Arrange
            var sut = new TaskService();
            var tasks = sut.GetAll();
            var now = DateTime.UtcNow;
            tasks[0].DueDate = now.AddDays(-1);
            tasks[1].DueDate = now.AddHours(-12);
            tasks[2].DueDate = now.AddDays(1);

            var criteria = new FilterCriteria
            {
                DueBefore = now
            };

            // Act
            var result = sut.GetFilteredTasks(criteria);

            // Assert
            Assert.Equal(2, result.Length);
            foreach (var task in result)
            {
                Assert.True(task.DueDate < now);
            }
        }

        [Fact]
        public void FilterTasks_WithNullCriteria_ReturnsAllTasks()
        {
            // Arrange
            var sut = new TaskService();
            var expected = sut.GetAll();

            // Act
            var result = sut.GetFilteredTasks(null);

            // Assert
            Assert.Equal(expected.Length, result.Length);
        }

        [Fact]
        public void FilterTasks_WithMultipleCriteria_ReturnsIntersection()
        {
            // Arrange
            var sut = new TaskService();
            var tasks = sut.GetAll();
            var limit = DateTime.UtcNow.AddDays(3);
            tasks[0].Status = Status.Completed;
            tasks[0].DueDate = limit.AddDays(-1);
            tasks[1].Status = Status.Completed;
            tasks[1].DueDate = limit.AddDays(1);
            tasks[2].Status = Status.InProgress;
            tasks[2].DueDate = limit.AddDays(-2);

            var criteria = new FilterCriteria
            {
                WithStatus = Status.Completed,
                DueBefore = limit
            };

            // Act
            var result = sut.GetFilteredTasks(criteria);

            // Assert
            Assert.Single(result);
            Assert.Equal(Status.Completed, result[0].Status);
            Assert.True(result[0].DueDate < limit);
        }

        [Fact]
        public void FilterTasks_NoMatches_ReturnsEmpty()
        {
            // Arrange
            var sut = new TaskService();
            var future = DateTime.UtcNow.AddYears(-5);

            var criteria = new FilterCriteria
            {
                DueBefore = future
            };

            // Act
            var result = sut.GetFilteredTasks(criteria);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void OrderTasks_ByDueDateAscending_UsesInsertionSort()
        {
            // Arrange
            var sut = new TaskService();
            var now = DateTime.UtcNow;
            var tasks = new[]
            {
                new TaskItem
                {
                    Id = 1,
                    Title = "B",
                    Description = "Description B",
                    DueDate = now.AddDays(3),
                    Status = Status.InProgress
                },
                new TaskItem
                {
                    Id = 2,
                    Title = "C",
                    Description = "Description C",
                    DueDate = now.AddDays(1),
                    Status = Status.NotStarted
                },
                new TaskItem
                {
                    Id = 3,
                    Title = "A",
                    Description = "Description A",
                    DueDate = now.AddDays(2),
                    Status = Status.Completed
                }
            };
            var criteria = new OrderCriteria
            {
                OrderBy = OrderByField.DueDate,
                Descending = false
            };

            // Act
            var result = sut.OrderTasks(tasks, criteria);

            // Assert
            Assert.Equal(3, result.Length);
            Assert.True(result[0].DueDate <= result[1].DueDate);
            Assert.True(result[1].DueDate <= result[2].DueDate);
        }

        [Fact]
        public void OrderTasks_ByDueDateDescending_ReversesOrder()
        {
            // Arrange
            var sut = new TaskService();
            var now = DateTime.UtcNow;
            var tasks = new[]
            {
                new TaskItem
                {
                    Id = 1,
                    Title = "B",
                    Description = "Description B",
                    DueDate = now.AddDays(1),
                    Status = Status.NotStarted
                },
                new TaskItem
                {
                    Id = 2,
                    Title = "A",
                    Description = "Description A",
                    DueDate = now.AddDays(3),
                    Status = Status.InProgress
                },
                new TaskItem
                {
                    Id = 3,
                    Title = "C",
                    Description = "Description C",
                    DueDate = now.AddDays(2),
                    Status = Status.Completed
                }
            };
            var criteria = new OrderCriteria
            {
                OrderBy = OrderByField.DueDate,
                Descending = true
            };

            // Act
            var result = sut.OrderTasks(tasks, criteria);

            // Assert
            Assert.Equal(3, result.Length);
            Assert.True(result[0].DueDate >= result[1].DueDate);
            Assert.True(result[1].DueDate >= result[2].DueDate);
        }

        [Fact]
        public void OrderTasks_ByTitleAscending_SortsAlphabetically()
        {
            // Arrange
            var sut = new TaskService();
            var tasks = new[]
            {
                new TaskItem
                {
                    Id = 1,
                    Title = "Charlie",
                    Description = "Description",
                    DueDate = DateTime.UtcNow,
                    Status = Status.InProgress
                },
                new TaskItem
                {
                    Id = 2,
                    Title = "alpha",
                    Description = "Description",
                    DueDate = DateTime.UtcNow,
                    Status = Status.Completed
                },
                new TaskItem
                {
                    Id = 3,
                    Title = "Bravo",
                    Description = "Description",
                    DueDate = DateTime.UtcNow,
                    Status = Status.NotStarted
                }
            };
            var criteria = new OrderCriteria
            {
                OrderBy = OrderByField.Title,
                Descending = false
            };

            // Act
            var result = sut.OrderTasks(tasks, criteria);

            // Assert
            Assert.Collection(
                result,
                task => Assert.Equal("alpha", task.Title),
                task => Assert.Equal("Bravo", task.Title),
                task => Assert.Equal("Charlie", task.Title));
        }

        [Fact]
        public void OrderTasks_ByStatusAscending_UsesEnumOrder()
        {
            // Arrange
            var sut = new TaskService();
            var tasks = new[]
            {
                new TaskItem
                {
                    Id = 1,
                    Title = "A",
                    Description = "Description",
                    DueDate = DateTime.UtcNow,
                    Status = Status.Completed
                },
                new TaskItem
                {
                    Id = 2,
                    Title = "B",
                    Description = "Description",
                    DueDate = DateTime.UtcNow,
                    Status = Status.NotStarted
                },
                new TaskItem
                {
                    Id = 3,
                    Title = "C",
                    Description = "Description",
                    DueDate = DateTime.UtcNow,
                    Status = Status.InProgress
                }
            };
            var criteria = new OrderCriteria
            {
                OrderBy = OrderByField.Status,
                Descending = false
            };

            // Act
            var result = sut.OrderTasks(tasks, criteria);

            // Assert
            Assert.Collection(
                result,
                task => Assert.Equal(Status.NotStarted, task.Status),
                task => Assert.Equal(Status.InProgress, task.Status),
                task => Assert.Equal(Status.Completed, task.Status));
        }

        [Fact]
        public void OrderTasks_WithNullField_PlacesNullLast()
        {
            // Arrange
            var sut = new TaskService();
            var tasks = new[]
            {
                new TaskItem
                {
                    Id = 1,
                    Title = "Alpha",
                    Description = "Description",
                    DueDate = DateTime.UtcNow,
                    Status = Status.NotStarted
                },
                new TaskItem
                {
                    Id = 2,
                    Title = null!,
                    Description = "Description",
                    DueDate = DateTime.UtcNow,
                    Status = Status.Completed
                },
                new TaskItem
                {
                    Id = 3,
                    Title = "Bravo",
                    Description = "Description",
                    DueDate = DateTime.UtcNow,
                    Status = Status.InProgress
                }
            };
            var criteria = new OrderCriteria
            {
                OrderBy = OrderByField.Title,
                Descending = false
            };

            // Act
            var result = sut.OrderTasks(tasks, criteria);

            // Assert
            Assert.Null(result[^1].Title);
            Assert.Equal("Alpha", result[0].Title);
            Assert.Equal("Bravo", result[1].Title);
        }

        [Fact]
        public void OrderTasks_WithEmptyArray_ReturnsEmptyArray()
        {
            // Arrange
            var sut = new TaskService();
            var tasks = Array.Empty<TaskItem>();
            var criteria = new OrderCriteria
            {
                OrderBy = OrderByField.DueDate
            };

            // Act
            var result = sut.OrderTasks(tasks, criteria);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void OrderTasks_WithIdenticalKeys_PreservesOriginalOrder()
        {
            // Arrange
            var sut = new TaskService();
            var dueDate = DateTime.UtcNow;
            var tasks = new[]
            {
                new TaskItem
                {
                    Id = 1,
                    Title = "Task1",
                    Description = "Description",
                    DueDate = dueDate,
                    Status = Status.NotStarted
                },
                new TaskItem
                {
                    Id = 2,
                    Title = "Task2",
                    Description = "Description",
                    DueDate = dueDate,
                    Status = Status.Completed
                },
                new TaskItem
                {
                    Id = 3,
                    Title = "Task3",
                    Description = "Description",
                    DueDate = dueDate,
                    Status = Status.InProgress
                }
            };
            var criteria = new OrderCriteria
            {
                OrderBy = OrderByField.DueDate,
                Descending = false
            };

            // Act
            var result = sut.OrderTasks(tasks, criteria);

            // Assert
            Assert.Collection(
                result,
                task => Assert.Equal(1, task.Id),
                task => Assert.Equal(2, task.Id),
                task => Assert.Equal(3, task.Id));
        }

        [Fact]
        public void OrderTasks_WithSingleItem_ReturnsSameItem()
        {
            // Arrange
            var sut = new TaskService();
            var task = new TaskItem
            {
                Id = 1,
                Title = "Only",
                Description = "Description",
                DueDate = DateTime.UtcNow,
                Status = Status.NotStarted
            };
            var tasks = new[] { task };
            var criteria = new OrderCriteria
            {
                OrderBy = OrderByField.Title
            };

            // Act
            var result = sut.OrderTasks(tasks, criteria);

            // Assert
            Assert.Single(result);
            Assert.Same(task, result[0]);
        }

        [Fact]
        public void OrderTasks_ByStatus_UsesCountingSort()
        {
            // Arrange
            var sut = new TaskService();
            var tasks = new[]
            {
                new TaskItem
                {
                    Id = 1,
                    Title = "A",
                    Description = "Description",
                    DueDate = DateTime.UtcNow,
                    Status = Status.Completed
                },
                new TaskItem
                {
                    Id = 2,
                    Title = "B",
                    Description = "Description",
                    DueDate = DateTime.UtcNow,
                    Status = Status.NotStarted
                },
                new TaskItem
                {
                    Id = 3,
                    Title = "C",
                    Description = "Description",
                    DueDate = DateTime.UtcNow,
                    Status = Status.InProgress
                }
            };
            var criteria = new OrderCriteria
            {
                OrderBy = OrderByField.Status,
                Descending = false
            };

            // Act
            var result = sut.OrderTasks(tasks, criteria);

            // Assert
            Assert.Collection(
                result,
                task => Assert.Equal(Status.NotStarted, task.Status),
                task => Assert.Equal(Status.InProgress, task.Status),
                task => Assert.Equal(Status.Completed, task.Status));
        }

        [Fact]
        public void OrderTasks_ByStatus_IsStableWithinSameBucket()
        {
            // Arrange
            var sut = new TaskService();
            var tasks = new[]
            {
                new TaskItem
                {
                    Id = 1,
                    Title = "A",
                    Description = "Description",
                    DueDate = DateTime.UtcNow,
                    Status = Status.NotStarted
                },
                new TaskItem
                {
                    Id = 2,
                    Title = "B",
                    Description = "Description",
                    DueDate = DateTime.UtcNow,
                    Status = Status.NotStarted
                },
                new TaskItem
                {
                    Id = 3,
                    Title = "C",
                    Description = "Description",
                    DueDate = DateTime.UtcNow,
                    Status = Status.InProgress
                },
                new TaskItem
                {
                    Id = 4,
                    Title = "D",
                    Description = "Description",
                    DueDate = DateTime.UtcNow,
                    Status = Status.InProgress
                }
            };
            var criteria = new OrderCriteria
            {
                OrderBy = OrderByField.Status,
                Descending = false
            };

            // Act
            var result = sut.OrderTasks(tasks, criteria);

            // Assert
            Assert.Collection(
                result,
                task => Assert.Equal(1, task.Id),
                task => Assert.Equal(2, task.Id),
                task => Assert.Equal(3, task.Id),
                task => Assert.Equal(4, task.Id));
        }

        [Fact]
        public void OrderTasks_ByStatusDescending_UsesReverseEnumOrder()
        {
            // Arrange
            var sut = new TaskService();
            var tasks = new[]
            {
                new TaskItem
                {
                    Id = 1,
                    Title = "A",
                    Description = "Description",
                    DueDate = DateTime.UtcNow,
                    Status = Status.NotStarted
                },
                new TaskItem
                {
                    Id = 2,
                    Title = "B",
                    Description = "Description",
                    DueDate = DateTime.UtcNow,
                    Status = Status.Completed
                },
                new TaskItem
                {
                    Id = 3,
                    Title = "C",
                    Description = "Description",
                    DueDate = DateTime.UtcNow,
                    Status = Status.InProgress
                },
                new TaskItem
                {
                    Id = 4,
                    Title = "D",
                    Description = "Description",
                    DueDate = DateTime.UtcNow,
                    Status = Status.Completed
                },
                new TaskItem
                {
                    Id = 5,
                    Title = "E",
                    Description = "Description",
                    DueDate = DateTime.UtcNow,
                    Status = Status.NotStarted
                },
                new TaskItem
                {
                    Id = 6,
                    Title = "F",
                    Description = "Description",
                    DueDate = DateTime.UtcNow,
                    Status = Status.InProgress
                }
            };
            var criteria = new OrderCriteria
            {
                OrderBy = OrderByField.Status,
                Descending = true
            };

            // Act
            var result = sut.OrderTasks(tasks, criteria);

            // Assert
            Assert.Collection(
                result,
                task => Assert.Equal(Status.Completed, task.Status),
                task => Assert.Equal(Status.Completed, task.Status),
                task => Assert.Equal(Status.InProgress, task.Status),
                task => Assert.Equal(Status.InProgress, task.Status),
                task => Assert.Equal(Status.NotStarted, task.Status),
                task => Assert.Equal(Status.NotStarted, task.Status));
        }
    }
}