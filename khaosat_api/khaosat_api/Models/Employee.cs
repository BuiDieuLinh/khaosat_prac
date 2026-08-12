using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace khaosat_api.Models
{
    public class Employee
    {
        public Guid Id { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }

        public Guid? PositionId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PermissionVersion { get; set; }
    }

    public class EmployeeResponse
    {
        public Guid Id { get; set; }
        [Display(Name = "Mã nhân viên")]
        [Required(ErrorMessage = "Mã nhân viên không được để trống")]
        [StringLength(8, MinimumLength = 2, ErrorMessage = "Độ dài phải từ 2 đến 8 ký tự")]
        public string EmployeeCode { get; set; } = string.Empty;

        [Display(Name = "Tên đầy đủ")]
        [Required(ErrorMessage = "Tên đầy đủ không được để trống")]
        [RegularExpression("^[^0-9]+$", ErrorMessage = "Tên đầy đủ không chứa ký tự số.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [RegularExpression(@"^[\d\w._-]+@[\d\w._-]+\.[\w]+$", ErrorMessage = "Email không hợp lệ")]
        [Remote("CheckEmailAddress", "RemoteValidation", ErrorMessage = "Email đã được đăng ký")]
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }

        public Guid? PositionId { get; set; }
        public string? PositionName { get; set; }
        public string? PositionCode { get; set; }

        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public string? DepartmentCode { get; set; }

        public List<Role> Roles { get; set; } = new List<Role>();
        public string RoleNames => string.Join(", ", Roles.Select(r => r.RoleName));

        public List<Guid> RoleIds => Roles.Select(r => r.Id).ToList();
        public Guid? PermissionVersion { get; set; }
    }

    public class Department
    {
        public Guid Id { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string DepartmentCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<PositionReps> Positions { get; set; } = new List<PositionReps>();
    }

    public class PositionReps
    {
        public Guid Id { get; set; }
        public string PositionName { get; set; } = string.Empty;
        public string PositionCode { get; set; } = string.Empty;
    }

    public class Position
    {
        public Guid Id { get; set; }
        public string PositionName { get; set; } = string.Empty;
        public string PositionCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid DepartmentId { get; set; }
    }

    public class Role
    {
        public Guid Id { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }   
}
