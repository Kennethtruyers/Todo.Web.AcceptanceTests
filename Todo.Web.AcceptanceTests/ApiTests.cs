using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using RestSharp;
using Xunit;

namespace Todo.Web.AcceptanceTests
{
    public static class Extensions
    {
        public static T Post<T>(this IRestClient client, string url, object request) where T : new()
        {
            var req = new RestRequest(url + "/") { RequestFormat = DataFormat.Json };
            req.AddBody(request);
            return client.Post<T>(req).Data;
        }

        public static T Put<T>(this IRestClient client, string url, object request) where T : new()
        {
            var req = new RestRequest(url + "/") { RequestFormat = DataFormat.Json };
            req.AddBody(request);
            return client.Put<T>(req).Data;
        }

        public static T Get<T>(this IRestClient client, string url) where T : new() =>
            client.Get<T>(new RestRequest(url + "/") ).Data;

        public static void Delete(this IRestClient client, string url) =>
            client.Delete(new RestRequest(url + "/"));
    }


    public class TaskServiceTestBase
    {
        static readonly string Environment = System.Environment.GetEnvironmentVariable("environment") ?? "dev";
        readonly string _baseurl = $"http://{Environment}-todo-web.azurewebsites.net/Tasks/";
        protected readonly IRestClient Client;
        protected readonly string User = Guid.NewGuid().ToString("N") + "@test.com";
        protected const string OtherUser = "othertest@test.com";
        protected TaskServiceTestBase()
        {
            Client = new RestClient(_baseurl);
        }
    }
    public class CreatingANewTask : TaskServiceTestBase
    {
        readonly Task _newTask;
        public CreatingANewTask()
        {
            _newTask = Client.Post<Task>(User, new Task { Name = "test", DueDate = DateTime.Now.AddDays(1) });
        }

        [Fact]
        public void ReturnsTheNewTask() => _newTask.Should().NotBeNull();
        [Fact]
        public void SetsTheIdOfTheTask() => _newTask.Id.Should().NotBe(0);
        [Fact]
        public void CanRetrieveTheTaskForThatUser() => Client.Get<List<Task>>(User).Should().Contain(t => t.Id == _newTask.Id);
        [Fact]
        public void CanRetrieveTheTaskByIdForThatUser() => Client.Get<Task>(User + "/" + _newTask.Id).Should().NotBeNull();
        [Fact]
        public void CannotRetrieveTheTaskForAnotherUser() => Client.Get<Task>(OtherUser + "/" + _newTask.Id).Should().BeNull();
    }

    public class CreatingMultipleTasks : TaskServiceTestBase
    {
        readonly List<Task> _tasks;
        public CreatingMultipleTasks()
        {
            _tasks = Enumerable.Range(0, 5)
                               .Select(i => Client.Post<Task>(User, new Task { Name = "test" + i, DueDate = DateTime.Now.AddDays(1) }))
                               .ToList();
        }

        [Fact]
        public void ReturnsAllTasks() => _tasks.Should().NotContainNulls();
        [Fact]
        public void SetsTheIdOfAllTasks() => _tasks.ForEach(t => t.Id.Should().NotBe(0));
        [Fact]
        public void CreatesUniqueIds() => _tasks.Select(t => t.Id).Should().OnlyHaveUniqueItems();
    }

    public class UpdatingATask : TaskServiceTestBase
    {
        readonly Task _initialTask;
        readonly Task _updatedTask;
        const string NewName = "test2";
        readonly DateTime _newDueDate = DateTime.Now.AddDays(2);
        public UpdatingATask()
        {
            _initialTask = Client.Post<Task>(User, new Task { Name = "test", DueDate = DateTime.Now.AddDays(1) });
            _updatedTask = Client.Put<Task>(User, new Task { Id = _initialTask.Id, Name = NewName, DueDate = _newDueDate });

        }
        [Fact]
        public void ReturnsTheUpdatedTask() => _updatedTask.Should().NotBeNull();
        [Fact]
        public void DoesNotModifyTheTaskId() => _updatedTask.Id.Should().Be(_initialTask.Id);
        [Fact]
        public void UpdatesTheName() => _updatedTask.Name.Should().Be(NewName);
        [Fact]
        public void UpdatesTheDueDate() => _updatedTask.DueDate.Should().Be(_newDueDate);
    }

    public class CompletingATask : TaskServiceTestBase
    {
        readonly Task _task;
        public CompletingATask()
        {
            _task = Client.Post<Task>(User, new Task { Name = "test", DueDate = DateTime.Now.AddDays(1) });
            _task.Done = true;
            Client.Put<Task>(User, _task);

        }
        [Fact]
        public void ReturnsTheTaskIdInTheEntireList() => Client.Get<List<Task>>(User).Should().Contain(t => t.Id == _task.Id);
        [Fact]
        public void ReturnsTheTaskIdInTheCompleteList() => Client.Get<List<Task>>(User + "/done").Should().Contain(t => t.Id == _task.Id);
        [Fact]
        public void DoesNotReturnTheTaskInTheOverdueList() => Client.Get<List<Task>>(User + "/overdue").Should().NotContain(t => t.Id == _task.Id);
        [Fact]
        public void DoesNotReturnTheTaskInThePendingList() => Client.Get<List<Task>>(User + "/pending").Should().NotContain(t => t.Id == _task.Id);
    }

    public class CreatingAnOverdueTask : TaskServiceTestBase
    {
        readonly Task _task;
        public CreatingAnOverdueTask()
        {
            _task = Client.Post<Task>(User, new Task { Name = "test", DueDate = DateTime.Now.AddDays(-1) });
        }
        [Fact]
        public void ReturnsTheTaskIdInTheEntireList() => Client.Get<List<Task>>(User).Should().Contain(t => t.Id == _task.Id);
        [Fact]
        public void ReturnsTheTaskIdInTheCompleteList() => Client.Get<List<Task>>(User + "/done").Should().NotContain(t => t.Id == _task.Id);
        [Fact]
        public void ReturnsTheTaskIdInTheOverdueList() => Client.Get<List<Task>>(User + "/overdue").Should().Contain(t => t.Id == _task.Id);
        [Fact]
        public void ReturnsTheTaskIdInThePendingList() => Client.Get<List<Task>>(User + "/pending").Should().Contain(t => t.Id == _task.Id);
    }

    public class CreatingAPendingTaskThatIsNotOverdue : TaskServiceTestBase
    {
        readonly Task _task;
        public CreatingAPendingTaskThatIsNotOverdue()
        {
            _task = Client.Post<Task>(User, new Task { Name = "test", DueDate = DateTime.Now.AddDays(1) });
        }
        [Fact]
        public void ReturnsTheTaskIdInTheEntireList() => Client.Get<List<Task>>(User).Should().Contain(t => t.Id == _task.Id);
        [Fact]
        public void ReturnsTheTaskIdInTheCompleteList() => Client.Get<List<Task>>(User + "/done").Should().NotContain(t => t.Id == _task.Id);
        [Fact]
        public void DoesNotReturnTheTaskInTheOverdueList() => Client.Get<List<Task>>(User + "/overdue").Should().NotContain(t => t.Id == _task.Id);
        [Fact]
        public void ReturnsTheTaskIdInThePendingList() => Client.Get<List<Task>>(User + "/pending").Should().Contain(t => t.Id == _task.Id);
    }

    public class DeletingATask : TaskServiceTestBase
    {
        readonly Task _initialTask;
        public DeletingATask()
        {
            _initialTask = Client.Post<Task>(User, new Task { Name = "test", DueDate = DateTime.Now.AddDays(1) });
            Client.Delete(User + "/" + _initialTask.Id);

        }
        [Fact]
        public void RemovesTask() => Client.Get<Task>(User + "/" + _initialTask.Id).Should().BeNull();
    }

    public class Task
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DueDate { get; set; }
        public bool Done { get; set; }
    }
}
