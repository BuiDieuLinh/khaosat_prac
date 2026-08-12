using khaosat_fe.Validation;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace khaosat_fe.Models
{
    public class EmployeeViewModel
    {
        public Guid Id { get; set; }

        [EmployeeValidation.EmployeeCode]
        [Display(Name = "Mã NV")]
        [Required(ErrorMessage = "Mã nhân viên không được để trống")]
        [RegularExpression(@"^EMP.*$", ErrorMessage = "Mã nhân viên phải bắt đầu bằng EMP")]
        [Remote("CheckEmployeeCodeExists", "Employee", ErrorMessage = "Mã nhân viên đã tồn tại")]
        public string EmployeeCode { get; set; } = string.Empty;

        [EmployeeValidation.FullName]
        [Display(Name = "Tên đầy đủ")]
        [Required(ErrorMessage = "Tên đầy đủ không được để trống")]
        [RegularExpression(@"^[^0-9]+$", ErrorMessage = "Không sử dụng ký tự số trong tên.")]
        public string FullName { get; set; } = string.Empty;

        [EmployeeValidation.CompanyEmail("oos.com.vn")]
        [Required(ErrorMessage = "Email không được để trống")]
        [RegularExpression(@"^[\d\w._-]+@oos\.com\.vn$", ErrorMessage = "Email phải thuộc domain @oos.com.vn")]
        [Remote("CheckEmailExists", "Employee", AdditionalFields = nameof(EmployeeCode), ErrorMessage = "Email đã được đăng ký")]
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Chức vụ")]
        [Required(ErrorMessage = "Chức vụ không được để trống")]
        public Guid? PositionId { get; set; }
        public string? PositionName { get; set; }
        public string? PositionCode { get; set; }

        [Display(Name = "Phòng ban")]
        [Required(ErrorMessage = "Phòng ban không được để trống")]
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public string? DepartmentCode { get; set; }

        public List<RoleViewModel> Roles { get; set; } = new List<RoleViewModel>();
        public string RoleNames => string.Join(", ", Roles.Select(r => r.RoleName));
        [Display(Name = "Phân quyền")]
        public List<Guid> RoleIds { get; set; } = new();
    }

    public class PositionViewModel
    {
        public Guid Id { get; set; }
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
        public List<PositionRepsViewModel> Positions { get; set; } = new List<PositionRepsViewModel>();
    }

    public class PositionRepsViewModel
    {
        public Guid Id { get; set; }
        public string? PositionName { get; set; }
        public string? PositionCode { get; set; }
    }

    public class RoleViewModel
    {
        public Guid Id { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }

    public class EmployeeFilterViewModel
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchKeyword { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PositionId { get; set; }
        public bool? IsActive { get; set; }
        public string? SortBy { get; set; } = "CreatedDate";
        public bool IsDescending { get; set; } = true;
    }

    //public class PagedResultViewModel<T>
    //{
    //    public List<T> Data { get; set; } = new List<T>();
    //    public int TotalCount { get; set; }
    //    public int PageNumber { get; set; }
    //    public int PageSize { get; set; }
    //    public int TotalPages { get; set; }
    //    public bool HasPreviousPage { get; set; }
    //    public bool HasNextPage { get; set; }
    //}
}
