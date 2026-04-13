using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace KlangIT_V3.Validation
{
    public class NumericOnlyAttribute : ValidationAttribute, IClientModelValidator
    {
        public NumericOnlyAttribute()
        {
            ErrorMessage = "ต้องเป็นตัวเลข";
        }

        // Server-side
        protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
        {
            if (value is null || string.IsNullOrEmpty(value.ToString()))
                return ValidationResult.Success;

            return value.ToString()!.All(char.IsDigit)
                ? ValidationResult.Success
                : new ValidationResult(ErrorMessage);
        }

        // Client-side
        public void AddValidation(ClientModelValidationContext context)
        {
            context.Attributes["data-val"] = "true";
            context.Attributes["data-val-numericonly"] = ErrorMessage!;
        }
    }
}