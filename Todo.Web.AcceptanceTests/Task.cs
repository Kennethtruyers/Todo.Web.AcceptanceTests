using System;

namespace Todo.Web.AcceptanceTests.Tests
{
    public class Task
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DueDate { get; set; }
        public bool Done { get; set; }
    }
}