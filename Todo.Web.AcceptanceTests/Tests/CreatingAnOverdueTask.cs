using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Todo.Web.AcceptanceTests.Tests
{
    public class CreatingAnOverdueTask : TaskServiceTestBase
    {
        readonly Task _task;
        public CreatingAnOverdueTask()
        {
            _task = Client.Post<Task>(User, new Task { Name = "test", DueDate = DateTime.Now.AddDays(-1) });
        }
        [Fact]
        public void ReturnsTheTaskIdInTheEntireList() =>
            Client.Get<List<Task>>(User).Should().Contain(t => t.Id == _task.Id);
        [Fact]
        public void ReturnsTheTaskIdInTheCompleteList() =>
            Client.Get<List<Task>>(User + "/done").Should().NotContain(t => t.Id == _task.Id);
        [Fact]
        public void ReturnsTheTaskIdInTheOverdueList() =>
            Client.Get<List<Task>>(User + "/overdue").Should().Contain(t => t.Id == _task.Id);
        [Fact]
        public void ReturnsTheTaskIdInThePendingList() =>
            Client.Get<List<Task>>(User + "/pending").Should().Contain(t => t.Id == _task.Id);
    }
}
