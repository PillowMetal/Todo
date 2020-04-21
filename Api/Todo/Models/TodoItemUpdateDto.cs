using System.ComponentModel.DataAnnotations;

namespace Todo.Models
{
    public class TodoItemUpdateDto
    {
        [Required]
        public string Name { get; set; }

        public string Project { get; set; }
        public string Context { get; set; }

        [Required]
        public bool IsComplete { get; set; }
    }
}
