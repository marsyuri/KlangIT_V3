using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace KlangIT_V3.Validation
{
    public class FixedLengthIfProvidedAttribute : ValidationAttribute, IClientModelValidator
    {
        private readonly int _length;

        public FixedLengthIfProvidedAttribute(int length)
        {
            _length = length;
            ErrorMessage = $"ต้องเป็นเลข {length} หลัก";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
        {
            if (value is null || string.IsNullOrEmpty(value.ToString()))
                return ValidationResult.Success; // empty is OK

            return value.ToString()!.Length == _length
                ? ValidationResult.Success
                : new ValidationResult(ErrorMessage);
        }

        // Client-side — inject HTML attributes ให้ jQuery Validation อ่านได้
        public void AddValidation(ClientModelValidationContext context)
        {
            context.Attributes["data-val"] = "true";
            context.Attributes["data-val-fixedlength"] = ErrorMessage ?? $"ต้องเป็นเลข {_length} หลัก";
            context.Attributes["data-val-fixedlength-length"] = _length.ToString();
        }
    }
}