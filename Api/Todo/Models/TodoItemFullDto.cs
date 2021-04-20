using System;

namespace Todo.Models
{
    public class TodoItemFullDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; }
        public string Project { get; init; }
        public string Context { get; init; }
        public DateTime Date { get; init; }
        public bool IsComplete { get; init; }
    }
}
