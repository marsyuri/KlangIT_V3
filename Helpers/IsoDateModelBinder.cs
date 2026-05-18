using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace KlangIT_V3.Helpers
{
    public class IsoDateModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null) throw new ArgumentNullException(nameof(bindingContext));

            var valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueResult == ValueProviderResult.None)
                return Task.CompletedTask;

            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueResult);

            var raw = valueResult.FirstValue;
            var nullable = Nullable.GetUnderlyingType(bindingContext.ModelType) != null;

            if (string.IsNullOrWhiteSpace(raw))
            {
                bindingContext.Result = nullable
                    ? ModelBindingResult.Success(null)
                    : ModelBindingResult.Failed();
                return Task.CompletedTask;
            }

            if (DateTime.TryParseExact(raw, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
             || DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                bindingContext.Result = ModelBindingResult.Success(date);
            }
            else
            {
                bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "รูปแบบวันที่ไม่ถูกต้อง");
                bindingContext.Result = ModelBindingResult.Failed();
            }

            return Task.CompletedTask;
        }
    }
}
