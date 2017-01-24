using System;
using FluentAssertions;
using Xunit;

namespace Todo.Web.AcceptanceTests.Tests
{
    public class UpdatingATask : TaskServiceTestBase
    {
        readonly Task _initialTask;
        readonly Task _updatedTask;
        const string NewName = "test2";
        readonly DateTime _newDueDate = DateTime.Now.AddDays(2);
        public UpdatingATask()
        {
            _initialTask = Client.Post<Task>(User, 
                new Task { Name = "test", DueDate = DateTime.Now.AddDays(1) });

            _updatedTask = Client.Put<Task>(User, 
                new Task { Id = _initialTask.Id, Name = NewName, DueDate = _newDueDate });
        }
        [Fact]
        public void ReturnsTheUpdatedTask() =>
            _updatedTask.Should().NotBeNull();
        [Fact]
        public void DoesNotModifyTheTaskId() =>
            _updatedTask.Id.Should().Be(_initialTask.Id);
        [Fact]
        public void UpdatesTheName() =>
            _updatedTask.Name.Should().Be(NewName);
        [Fact]
        public void UpdatesTheDueDate() =>
            _updatedTask.DueDate.Should().Be(_newDueDate);
    }
}
