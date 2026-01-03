using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WebApp.Model.Validation.CustomValidationAttributes;

namespace WebApp.Model.Request
{
    public class UserRequest
    {
        [Required(ErrorMessage = "UserName is required")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "UserName must be between 5 and 50 characters")]
        [RegularExpression(@"^[\p{L}\p{M}\s_ ]+$", ErrorMessage = "Full name can only contain letters, numbers, spaces and underscore")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Password is required")]
        [PasswordStrength(MinLength = 8, RequireUppercase = true, RequireLowercase = true, RequireDigit = true)]
        public string Password { get; set; }
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [PhoneNumberValidation(ErrorMessage = "Invalid phone number format. It should start with '0' followed by 9 or 10 digits.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Address cannot exceed 200 characters")]
        public string Address { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
