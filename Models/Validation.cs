using System.ComponentModel.DataAnnotations;
using CustomerAgreements.Models;

namespace CustomerAgreements.Validation
{
    public class RequiredIfIncludeInstructionsAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            var section = (Section)validationContext.ObjectInstance;

            if (section.IncludeInstructions && string.IsNullOrWhiteSpace(section.Instructions))
            {
                return new ValidationResult("Instructions are required when Include Instructions is set to Yes.");
            }

            return ValidationResult.Success!;
        }
    }
}

