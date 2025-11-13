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
    }
}