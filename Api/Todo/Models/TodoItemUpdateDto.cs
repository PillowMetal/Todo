using System.ComponentModel.DataAnnotations;

namespace Todo.Models
{
    public class TodoItemUpdateDto : TodoItemManipulationDto
    {
        [Required]
        public override string Project { get => base.Project; set => base.Project = value; }
    }
}
