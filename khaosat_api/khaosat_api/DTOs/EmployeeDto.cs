using khaosat_api.Models;
using System;
using System.Collections.Generic;

namespace khaosat_api.DTOs
{
    public class EmployeeResponseDto
    {
        public Guid Id { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }

        public Guid? PositionId { get; set; }
        public string? PositionName { get; set; }
        public string? Description { get; set; }
        public string? PositionCode { get; set; }

        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public string? DepartmentCode { get; set; }

        public List<Role> Roles { get; set; } = new List<Role>();
        public List<Guid> RoleIds { get; set; } = new();
    }

    public class EmployeeCreateDto
    {
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Password { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid? PositionId { get; set; }
        public List<Guid> RoleIds { get; set; } = new List<Guid>();
    }

    public class EmployeeUpdateDto
    {
        public Guid Id { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public Guid? PositionId { get; set; }
        public List<Guid> RoleIds { get; set; } = new List<Guid>();
    }

    public class DepartmentDto
    {
        public Guid Id { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string DepartmentCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class PositionDto
    {
        public Guid Id { get; set; }
        public string PositionName { get; set; } = string.Empty;
        public string PositionCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid DepartmentId { get; set; }
    }

    public class RoleDto
    {
        public Guid Id { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }
}
