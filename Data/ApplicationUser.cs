using Microsoft.AspNetCore.Identity;

namespace KlangIT_V3.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string? DisplayName { get; set; }
    }
}
