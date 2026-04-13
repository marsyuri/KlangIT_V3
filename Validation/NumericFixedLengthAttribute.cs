using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace KlangIT_V3.Validation
{
    public class NumericFixedLengthAttribute : ValidationAttribute, IClientModelValidator
    {
        private readonly int _length;

        public NumericFixedLengthAttribute(int length)
        {
            _length = length;
            ErrorMessage = $"ต้องเป็นเลข {length} หลัก";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
        {
            if (value is null || string.IsNullOrEmpty(value.ToString()))
                return ValidationResult.Success;

            var str = value.ToString()!;

            if (!str.All(char.IsDigit))
                return new ValidationResult($"ต้องเป็นตัวเลข");

            if (str.Length != _length)
                return new ValidationResult($"ต้องเป็นเลข {_length} หลัก");

            return ValidationResult.Success;
        }

        public void AddValidation(ClientModelValidationContext context)
        {
            context.Attributes["data-val"] = "true";
            context.Attributes["data-val-numericfixedlength"] = ErrorMessage!;
            context.Attributes["data-val-numericfixedlength-length"] = _length.ToString();
        }
    }
}
