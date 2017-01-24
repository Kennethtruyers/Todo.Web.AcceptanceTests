using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Todo.Web.AcceptanceTests.Tests
{
    public class CompletingATask : TaskServiceTestBase
    {
        readonly Task _task;
        public CompletingATask()
        {
            _task = Client.Post<Task>(User, 
                new Task { Name = "test", DueDate = DateTime.Now.AddDays(1) });
            _task.Done = true;
            Client.Put<Task>(User, _task);
        }
        [Fact]
        public void ReturnsTheTaskIdInTheEntireList() =>
            Client.Get<List<Task>>(User).Should().Contain(t => t.Id == _task.Id);
        [Fact]
        public void ReturnsTheTaskIdInTheCompleteList() =>
            Client.Get<List<Task>>(User + "/done").Should().Contain(t => t.Id == _task.Id);
        [Fact]
        public void DoesNotReturnTheTaskInTheOverdueList() =>
            Client.Get<List<Task>>(User + "/overdue").Should().NotContain(t => t.Id == _task.Id);
        [Fact]
        public void DoesNotReturnTheTaskInThePendingList() =>
            Client.Get<List<Task>>(User + "/pending").Should().NotContain(t => t.Id == _task.Id);
    }
}
