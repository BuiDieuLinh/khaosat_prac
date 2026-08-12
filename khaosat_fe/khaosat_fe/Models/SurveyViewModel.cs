using System.ComponentModel.DataAnnotations;

namespace khaosat_fe.Models
{
    public class SurveyTargetViewModel
    {
        public int TargetType { get; set; } // 0 = Company, 1 = Department, Position,  Employee
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
        public Guid EmployeeId { get; set; }
        public List<AnswerSubmitViewModel> Answers { get; set; } = new();
    }

    public class AnswerSubmitViewModel
    {
        public Guid ElementId { get; set; }
        public Guid? OptionId { get; set; }
        public string? Value { get; set; }
    }

    public class SurveyCreateNestedViewModel
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public byte Status { get; set; } = 1;
        public int? MaxAttempts { get; set; }
        public List<SurveyTargetViewModel> Targets { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndDate < StartDate)
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
}
