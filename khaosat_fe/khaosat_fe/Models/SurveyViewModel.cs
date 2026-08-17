using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace khaosat_fe.Models
{
    public class SurveyTargetViewModel
    {
        public int TargetType { get; set; } // 0 = Company, 1 = Department, Position, Employee
        public Guid? DepartmentId { get; set; }
        public Guid? PositionId { get; set; }
    }

    public class SurveyViewModel
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public byte Status { get; set; }
        public int? MaxAttempts { get; set; }
        public int AccessType { get; set; } = 1; // 1: Internal, 2: Public, 3: Invitation
        public bool AnonymousMode { get; set; } = false;
        public List<SurveyTargetViewModel> Targets { get; set; } = new();
        public int TotalResponses { get; set; }
        public int CompletedCount { get; set; }
        public int IncompleteCount { get; set; }
        public double CompletionRate { get; set; }
    }

    public class SurveyDetailViewModel
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public byte Status { get; set; }
        public int? MaxAttempts { get; set; }
        public int AccessType { get; set; } = 1;
        public bool AnonymousMode { get; set; } = false;
        public List<SurveyTargetViewModel> Targets { get; set; } = new();
        public List<SurveyElementDetailViewModel> Elements { get; set; } = new();
    }

    public class SurveyElementDetailViewModel
    {
        public Guid Id { get; set; }
        public Guid SurveyId { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public string ConfigType { get; set; } = string.Empty;
        public List<SurveyElementOptionViewModel> Options { get; set; } = new();
    }

    public class SurveyElementOptionViewModel
    {
        public Guid Id { get; set; }
        public Guid ElementId { get; set; }
        public string Value { get; set; } = string.Empty;
        public string DisplayText { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
    }

    public class SurveySubmitViewModel
    {
        public Guid SurveyId { get; set; }
        public Guid? EmployeeId { get; set; }
        public string? PublicToken { get; set; }
        public string? DeviceFingerprint { get; set; }
        public string? CookieId { get; set; }
        public List<AnswerSubmitViewModel> Answers { get; set; } = new();
    }

    public class AnswerSubmitViewModel
    {
        public Guid ElementId { get; set; }
        public Guid? OptionId { get; set; }
        public string? Value { get; set; }
    }

    public class SurveyCreateNestedViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Mã khảo sát không được để trống")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên khảo sát không được để trống")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime? EndDate { get; set; }
        public byte Status { get; set; } = 1;
        public int? MaxAttempts { get; set; }
        public int AccessType { get; set; } = 1; // 1: Internal, 2: Public, 3: Invitation
        public bool AnonymousMode { get; set; } = false;

        public List<SurveyTargetViewModel> Targets { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndDate.HasValue && EndDate < StartDate)
            {
                yield return new ValidationResult(
                    "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.",
                    new[] { nameof(EndDate) });
            }
        }

        public List<SurveyElementCreateNestedViewModel> Elements { get; set; } = new();
    }

    public class SurveyElementCreateNestedViewModel
    {
        public string FieldName { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public string ConfigType { get; set; } = string.Empty;
        public List<SurveyElementOptionCreateNestedViewModel> Options { get; set; } = new();
    }

    public class SurveyElementOptionCreateNestedViewModel
    {
        public string Value { get; set; } = string.Empty;
        public string DisplayText { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class SurveyFilterDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchKeyword { get; set; }
        public SurveyStatus? Status { get; set; }
        public string? SortBy { get; set; } = "CreatedDate";
        public bool IsDescending { get; set; } = true;
    }

    public enum SurveyStatus : byte
    {
        Draft = 0,
        Active = 1,
        Closed = 2
    }

    public class AuditLogViewModel
    {
        public Guid Id { get; set; }
        public string? UserName { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? EntityType { get; set; }
        public string? EntityId { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SurveyReportViewModel
    {
        public Guid SurveyId { get; set; }
        public string SurveyCode { get; set; } = string.Empty;
        public string SurveyName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public byte Status { get; set; }
        public int AccessType { get; set; }
        public bool AnonymousMode { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedDate { get; set; }

        public int TotalResponses { get; set; }
        public int TotalTargetParticipants { get; set; }
        public int CompletedParticipantsCount { get; set; }
        public double CompletionRate { get; set; }
        public string TargetSummary { get; set; } = string.Empty;

        public int MinimumThreshold { get; set; } = 5;
        public bool IsPrivacyThresholdMet { get; set; }
        public string? PrivacyMessage { get; set; }

        public List<QuestionReportViewModel> Questions { get; set; } = new();
        public List<DepartmentReportViewModel> DepartmentBreakdown { get; set; } = new();
        public List<GroupReportItemViewModel> GroupBreakdown { get; set; } = new();
    }

    public class QuestionReportViewModel
    {
        public Guid ElementId { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string Caption { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public int TotalAnswerCount { get; set; }

        public List<OptionReportViewModel> Options { get; set; } = new();
        public List<string> TextAnswers { get; set; } = new();
    }

    public class OptionReportViewModel
    {
        public Guid OptionId { get; set; }
        public string DisplayText { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class DepartmentReportViewModel
    {
        public Guid? DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int TotalAssigned { get; set; }
        public int CompletedCount { get; set; }
        public double CompletionRate { get; set; }
    }

    public class GroupReportItemViewModel
    {
        public string GroupName { get; set; } = string.Empty;
        public int ResponseCount { get; set; }
        public bool IsPrivacyThresholdMet { get; set; }
        public string? Note { get; set; }
    }

    public class ChangeAccessTypeViewModel
    {
        public int AccessType { get; set; }
    }

    public class ChangeAnonymousModeViewModel
    {
        public bool AnonymousMode { get; set; }
    }
}
