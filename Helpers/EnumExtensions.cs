using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace KlangIT_V3.Helpers
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum value)
        {
            return value.GetType()
                        .GetField(value.ToString())
                        ?.GetCustomAttribute<DisplayAttribute>()
                        ?.Name
                        ?? value.ToString();
        }
    }
}
