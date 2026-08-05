using System;

namespace khaosat_api.DTOs
{
    public class EmployeeLoginDto
    {
        public string Username { get; set; } = string.Empty; // Can be EmployeeCode or Email
        public string Password { get; set; } = string.Empty;
    }

    public class EmployeeRegisterDto
    {
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public Guid Id { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string PermissionVersion { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
    }
}
