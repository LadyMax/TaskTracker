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
    }
}