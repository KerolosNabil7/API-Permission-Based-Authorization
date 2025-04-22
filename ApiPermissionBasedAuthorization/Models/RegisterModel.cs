using System.ComponentModel.DataAnnotations;

namespace ApiPermissionBasedAuthorization.Models
{
    public class RegisterModel
    {
        [Required, MaxLength(50)]
        public string Username { get; set; }
        [Required, MaxLength(128)]
        public string Email { get; set; }
        [Required, MaxLength(256)]
        public string Password { get; set; }
    }
}
