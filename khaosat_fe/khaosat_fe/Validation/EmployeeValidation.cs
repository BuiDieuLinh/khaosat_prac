using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace khaosat_fe.Validation
{
    public class EmployeeValidation
    {
        public class EmployeeCodeAttribute : ValidationAttribute, IClientModelValidator
        {
            public EmployeeCodeAttribute()
            {
                ErrorMessage = "Mã nhân viên phải bắt đầu bằng EMP";
            }

            protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    return ValidationResult.Success;

                if (value.ToString()!.StartsWith("EMP"))
                    return ValidationResult.Success;

                return new ValidationResult(ErrorMessage);
            }

            public void AddValidation(ClientModelValidationContext context)
            {
                MergeAttribute(context.Attributes, "data-val", "true");
                MergeAttribute(context.Attributes, "data-val-regex", ErrorMessage ?? "Mã nhân viên phải bắt đầu bằng EMP");
                MergeAttribute(context.Attributes, "data-val-regex-pattern", @"^EMP.*$");
            }

            private static bool MergeAttribute(IDictionary<string, string> attributes, string key, string value)
            {
                if (attributes.ContainsKey(key))
                {
                    return false;
                }
                attributes.Add(key, value);
                return true;
            }
        }

        public class CompanyEmailAttribute : ValidationAttribute, IClientModelValidator
        {
            private readonly string _domain;

            public CompanyEmailAttribute(string domain = "oos.com.vn")
            {
                _domain = domain;
                ErrorMessage = $"Email phải thuộc domain @{_domain}";
            }

            protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    return ValidationResult.Success;

                var email = value.ToString()!;

                if (email.EndsWith($"@{_domain}", StringComparison.OrdinalIgnoreCase))
                    return ValidationResult.Success;

                return new ValidationResult(ErrorMessage);
            }

            public void AddValidation(ClientModelValidationContext context)
            {
                MergeAttribute(context.Attributes, "data-val", "true");
                MergeAttribute(context.Attributes, "data-val-regex", ErrorMessage ?? $"Email phải thuộc domain @{_domain}");
                MergeAttribute(context.Attributes, "data-val-regex-pattern", $@"^[\d\w._-]+@{Regex.Escape(_domain)}$");
            }

            private static bool MergeAttribute(IDictionary<string, string> attributes, string key, string value)
            {
                if (attributes.ContainsKey(key))
                {
                    return false;
                }
                attributes.Add(key, value);
                return true;
            }
        }

        public class FullNameAttribute : ValidationAttribute, IClientModelValidator
        {
            public FullNameAttribute()
            {
                ErrorMessage = "Họ tên chỉ được chứa chữ cái và khoảng trắng.";
            }

            protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    return ValidationResult.Success;

                var name = value.ToString()!;
                var regex = new Regex(@"^[^0-9]+$");

                if (regex.IsMatch(name))
                    return ValidationResult.Success;

                return new ValidationResult(ErrorMessage);
            }

            public void AddValidation(ClientModelValidationContext context)
            {
                MergeAttribute(context.Attributes, "data-val", "true");
                MergeAttribute(context.Attributes, "data-val-regex", ErrorMessage ?? "Họ tên chỉ được chứa chữ cái và khoảng trắng.");
                MergeAttribute(context.Attributes, "data-val-regex-pattern", @"^[^0-9]+$");
            }

            private static bool MergeAttribute(IDictionary<string, string> attributes, string key, string value)
            {
                if (attributes.ContainsKey(key))
                {
                    return false;
                }
                attributes.Add(key, value);
                return true;
            }
        }
    }
}
