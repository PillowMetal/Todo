using System;
using System.ComponentModel.DataAnnotations;
using static System.DateTime;

namespace Todo.Models
{
    public abstract class TodoItemManipulationDto
    {
        [Required]
        public string Name { get; set; }

        public virtual string Project { get; set; }
        public string Context { get; set; }
        public DateTime Date { get; set; } = Today;
        public bool IsComplete { get; set; }
    }
}
