using System.ComponentModel.DataAnnotations;

namespace Todo.Models
{
    public abstract class TodoItemManipulationDto
    {
        [Required]
        public string Name { get; set; }

        public virtual string Project { get; set; }
        public string Context { get; set; }
        public bool IsComplete { get; set; }
    }
}
