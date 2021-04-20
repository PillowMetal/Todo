using System;

namespace Todo.Models
{
    public class TodoItemDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; }
        public string Tags { get; init; }
        public int Age { get; init; }
        public bool IsComplete { get; init; }
    }
}
