using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace khaosat_fe.Models
{
    public class EmployeeViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Mã NV")]
        public string EmployeeCode { get; set; } = string.Empty;

        [Display(Name = "Tên đầy đủ")]
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Chức vụ")]
        public Guid? PositionId { get; set; }
        public string? PositionName { get; set; }
        public string? PositionCode { get; set; }

        [Display(Name = "Phòng ban")]
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public string? DepartmentCode { get; set; }

        public List<RoleViewModel> Roles { get; set; } = new List<RoleViewModel>();
        public string RoleNames => string.Join(", ", Roles.Select(r => r.RoleName));
        public List<Guid> RoleIds { get; set; } = new();
    }

    public class PositionViewModel
    {
        public Guid Id { get; set; }
        public Guid? PositionId { get => Id; set => Id = value ?? Guid.Empty; }
        public string? PositionName { get; set; }
        public string? PositionCode { get; set; }
        public string? Description { get; set; }
        public Guid? DepartmentId { get; set; }
    }

    public class DepartmentViewModel
    {
        public Guid Id { get; set; }
        public Guid? DepartmentId { get => Id; set => Id = value ?? Guid.Empty; }
        public string? DepartmentName { get; set; }
        public string? DepartmentCode { get; set; }
        public string? Description { get; set; }
    }

    public class RoleViewModel
    {
        public Guid Id { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }
}
