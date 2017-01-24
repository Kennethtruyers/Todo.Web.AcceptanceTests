using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Todo.Web.AcceptanceTests.Tests
{
    public class CreatingANewTask : TaskServiceTestBase
    {
        readonly Task _newTask;
        public CreatingANewTask()
        {
            _newTask = Client.Post<Task>(User, 
                new Task { Name = "test", DueDate = DateTime.Now.AddDays(1) });
        }

        [Fact]
        public void ReturnsTheNewTask() =>
            _newTask.Should().NotBeNull();
        [Fact]
        public void SetsTheIdOfTheTask() =>
            _newTask.Id.Should().NotBe(0);
        [Fact]
        public void CanRetrieveTheTaskForThatUser() =>
            Client.Get<List<Task>>(User).Should().Contain(t => t.Id == _newTask.Id);
        [Fact]
        public void CanRetrieveTheTaskByIdForThatUser() =>
            Client.Get<Task>(User + "/" + _newTask.Id).Should().NotBeNull();
        [Fact]
        public void CannotRetrieveTheTaskForAnotherUser() =>
            Client.Get<Task>(OtherUser + "/" + _newTask.Id).Should().BeNull();
    }
}
