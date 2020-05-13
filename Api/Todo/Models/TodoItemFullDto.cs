using System;

namespace Todo.Models
{
    public class TodoItemFullDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Project { get; set; }
        public string Context { get; set; }
        public DateTime Date { get; set; }
        public bool IsComplete { get; set; }
    }
}
