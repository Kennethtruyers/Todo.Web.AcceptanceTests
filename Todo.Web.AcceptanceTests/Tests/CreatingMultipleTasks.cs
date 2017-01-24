using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Todo.Web.AcceptanceTests.Tests
{
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
        public void ReturnsAllTasks() =>
            _tasks.Should().NotContainNulls();
        [Fact]
        public void SetsTheIdOfAllTasks() =>
            _tasks.ForEach(t => t.Id.Should().NotBe(0));
        [Fact]
        public void CreatesUniqueIds() =>
            _tasks.Select(t => t.Id).Should().OnlyHaveUniqueItems();
    }
}
