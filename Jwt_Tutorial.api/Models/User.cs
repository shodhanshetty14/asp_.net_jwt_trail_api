using System.ComponentModel.DataAnnotations;

namespace Jwt_Tutorial.api.Models
{
    public class User
    {
       
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required, MaxLength(20)]
        public string Password { get; set; } = string.Empty;
    }
}
