using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebApp.Model.Validation
{
    public class CustomValidationAttributes
    {
        public class PhoneNumberValidationAttribute : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                if ( value == null | string.IsNullOrWhiteSpace(value.ToString()) )
                {
                    return ValidationResult.Success;
                }

                var phone = value.ToString();
                var phoneRegex = @"^0\d{9,10}$";

                if (!Regex.IsMatch(phone, phoneRegex))
                {
                    return new ValidationResult("Invalid phone number format. It should start with '0' followed by 9 or 10 digits.");
                }

                return ValidationResult.Success;
            }
        }

        public class  PasswordStrengthAttribute : ValidationAttribute
        {
            public int MinLength { get; set; } = 8;
            public bool RequireUppercase { get; set; } = true;
            public bool RequireLowercase { get; set; } = true;
            public bool RequireDigit { get; set; } = true;
            public bool RequireSpecialChar { get; set; } = false;

            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var password = value.ToString();

                if (password.Length < MinLength)
                {
                    return new ValidationResult($"Password must be at least {MinLength} characters long");
                }

                if (RequireUppercase && !password.Any(char.IsUpper))
                {
                    return new ValidationResult("Password must contain at least one uppercase letter");
                }

                if (RequireLowercase && !password.Any(char.IsLower))
                {
                    return new ValidationResult("Password must contain at least one lowercase letter");
                }

                if (RequireDigit && !password.Any(char.IsDigit))
                {
                    return new ValidationResult("Password must contain at least one digit");
                }

                if (RequireSpecialChar && !password.Any(ch => !char.IsLetterOrDigit(ch)))
                {
                    return new ValidationResult("Password must contain at least one special character");
                }

                return ValidationResult.Success;
            }
        }

        public class AgeRangeAttribute : ValidationAttribute
        {
            public int MinAge { get; set; } = 18;
            public int MaxAge { get; set; } = 120;

            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                if (value == null)
                {
                    return ValidationResult.Success;
                }

                if (value is DateTime dateOfBirth)
                {
                    var age = DateTime.Today.Year - dateOfBirth.Year;
                    if (dateOfBirth.Date > DateTime.Today.AddYears(-age)) age--;

                    if (age < MinAge || age > MaxAge)
                    {
                        return new ValidationResult($"Age must be between {MinAge} and {MaxAge}");
                    }
                    return ValidationResult.Success;
                }

                return new ValidationResult("Invalid date of birth");
            }
        }
    }
}
