using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static System.DateTime;

namespace Todo.Models
{
    public abstract class TodoItemManipulationDto : IValidatableObject
    {
        [Required]
        public string Name { get; init; }

        public virtual string Project { get; init; }
        public string Context { get; init; }
        public DateTime Date { get; init; } = Today;
        public bool IsComplete { get; init; }

        #region Implementation of IValidatableObject

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Project == Context)
                yield return new ValidationResult("The provided project must be different than the context.", new[] { nameof(TodoItemManipulationDto) });
        }

        #endregion
    }
}
