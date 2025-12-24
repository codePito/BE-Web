using System.ComponentModel.DataAnnotations;
using static WebApp.Model.Validation.CustomValidationAttributes;

public class UpdateProfileRequest
{
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(50, MinimumLength = 5, ErrorMessage = "Full name must be between 5 and 50 characters")]
    [RegularExpression(@"^[\p{L}\p{M}\d\s_]+$", ErrorMessage = "Full name can only contain letters, numbers, spaces and underscore")]
    public string UserName { get; set; }

    [Required(ErrorMessage = "Phone number is required")]
    [PhoneNumberValidation(ErrorMessage = "Invalid phone number. Must start with 0 and have 10-11 digits (e.g., 0123456789)")]
    public string PhoneNumber { get; set; }

    [Required(ErrorMessage = "Address is required")]
    [StringLength(200, MinimumLength = 10, ErrorMessage = "Address must be between 10 and 200 characters")]
    public string Address { get; set; } = string.Empty;
}