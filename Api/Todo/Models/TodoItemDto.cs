using System;

namespace Todo.Models
{
    public class TodoItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Tags { get; set; }
        public int Age { get; set; }
        public bool IsComplete { get; set; }
    }
}
