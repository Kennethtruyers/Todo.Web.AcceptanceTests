using System;
using FluentAssertions;
using Xunit;

namespace Todo.Web.AcceptanceTests.Tests
{
    public class DeletingATask : TaskServiceTestBase
    {
        readonly Task _initialTask;
        public DeletingATask()
        {
            _initialTask = Client.Post<Task>(User, new Task { Name = "test", DueDate = DateTime.Now.AddDays(1) });
            Client.Delete(User + "/" + _initialTask.Id);

        }
        [Fact]
        public void RemovesTask() =>
            Client.Get<Task>(User + "/" + _initialTask.Id).Should().BeNull();
    }
}
