using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace khaosat_fe.Validation
{
    public class EmployeeValidation
    {
        public class EmployeeCodeAttribute : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext context)
            {
                if (value == null)
                    return ValidationResult.Success;

                if (value.ToString().StartsWith("EMP"))
                    return ValidationResult.Success;

                return new ValidationResult("Mã nhân viên phải bắt đầu bằng EMP");
            }
        }

        public class EmailAttribute : ValidationAttribute
        {
            private readonly string _domain;

            public EmailAttribute(string domain = "company.com")
            {
                _domain = domain;
                ErrorMessage = $"Email phải thuộc domain @{_domain}.";
            }

            protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
            {
                if (value == null)
                    return ValidationResult.Success;

                var email = value.ToString();

                if (string.IsNullOrWhiteSpace(email))
                    return ValidationResult.Success;

                if (email.EndsWith($"@{_domain}", StringComparison.OrdinalIgnoreCase))
                    return ValidationResult.Success;

                return new ValidationResult(ErrorMessage);
            }
        }

        public class FullNameAttribute : ValidationAttribute
        {
            protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
            {
                if (value == null)
                    return ValidationResult.Success;

                var name = value.ToString();

                if (string.IsNullOrWhiteSpace(name))
                    return ValidationResult.Success;

                var regex = new Regex(@"^[\p{L}\s]+$");

                if (regex.IsMatch(name))
                    return ValidationResult.Success;

                return new ValidationResult("Họ tên chỉ được chứa chữ cái và khoảng trắng.");
            }
        }
    }
}
