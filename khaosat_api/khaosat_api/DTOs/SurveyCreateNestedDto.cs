using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace khaosat_api.DTOs
{
    public class SurveyCreateNestedDto : IValidatableObject
    {
        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public byte Status { get; set; } = 1;

        public int? MaxAttempts { get; set; }

        public List<SurveyTargetDto> Targets { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndDate < StartDate)
            {
                yield return new ValidationResult(
                    "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.",
                    new[] { nameof(EndDate) });
            }
        }

        public List<SurveyElementCreateNestedDto> Elements { get; set; } = new();
    }

    public class SurveyElementCreateNestedDto
    {
        [Required]
        public string FieldName { get; set; } = string.Empty;

        public int SortOrder { get; set; }

        [Required]
        public string ConfigType { get; set; } = string.Empty;

        public List<SurveyElementOptionCreateNestedDto> Options { get; set; } = new();
    }

    public class SurveyElementOptionCreateNestedDto
    {
        [Required]
        public string Value { get; set; } = string.Empty;

        [Required]
        public string DisplayText { get; set; } = string.Empty;

        public int SortOrder { get; set; }

        public bool IsDefault { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
