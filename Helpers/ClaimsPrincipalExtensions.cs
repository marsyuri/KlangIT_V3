using KlangIT_V3.Data;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace KlangIT_V3.Helpers
{
    public static class ClaimsPrincipalExtensions
    {
        public static async Task<string> GetDisplayNameAsync(
            this ClaimsPrincipal principal,
            UserManager<ApplicationUser> userManager)
        {
            var name = principal?.Identity?.Name;
            if (string.IsNullOrEmpty(name)) return string.Empty;

            var user = await userManager.FindByNameAsync(name);
            if (!string.IsNullOrWhiteSpace(user?.DisplayName))
                return user!.DisplayName!;

            int at = name.IndexOf('@');
            return at > 0 ? name[..at] : name;
        }
    }
}
