using System.ComponentModel.DataAnnotations;

namespace PolicyAuthWebAPI.Models
{
    public record RegisterModel([Required] string Email, [Required] string Role, [Required] DateTime DateOfBirth, [Required] string Password);

}
