using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static System.DateTime;

namespace Todo.Models
{
    public abstract class TodoItemManipulationDto : IValidatableObject
    {
        [Required]
        public string Name { get; set; }

        public virtual string Project { get; set; }
        public string Context { get; set; }
        public DateTime Date { get; set; } = Today;
        public bool IsComplete { get; set; }

        #region Implementation of IValidatableObject

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Project == Context)
                yield return new ValidationResult("The provided project must be different than the context.", new[] { nameof(TodoItemManipulationDto) });
        }

        #endregion
    }
}
