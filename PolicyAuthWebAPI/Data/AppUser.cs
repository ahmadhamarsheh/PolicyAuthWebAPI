using Microsoft.AspNetCore.Identity;

namespace PolicyAuthWebAPI.Data
{
    public class AppUser : IdentityUser
    {
        public DateTime DateOfBirth { get; set; }
    }
}
