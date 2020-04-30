using System;
using System.ComponentModel.DataAnnotations;

namespace Todo.Entities
{
    public class TodoItem
    {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Project { get; set; }
        public string Context { get; set; }
        public DateTime Date { get; set; }
        public bool IsComplete { get; set; }
        public string Secret { get; set; }
    }
}
