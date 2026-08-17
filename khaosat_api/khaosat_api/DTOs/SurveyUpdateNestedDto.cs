using khaosat_api.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace khaosat_api.DTOs
{
    public class SurveyUpdateNestedDto : IValidatableObject
    {
        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public SurveyStatus Status { get; set; } = (SurveyStatus)1;

        public int? MaxAttempts { get; set; }

        public int AccessType { get; set; } = 1; // 1: Internal, 2: Public, 3: Invitation

        public bool AnonymousMode { get; set; } = false;

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

        public List<SurveyElementUpdateNestedDto> Elements { get; set; } = new();
    }

    public class SurveyElementUpdateNestedDto
    {
        [Required]
        public string FieldName { get; set; } = string.Empty;

        public int SortOrder { get; set; }

        [Required]
        public string ConfigType { get; set; } = string.Empty;

        public List<SurveyElementOptionUpdateNestedDto> Options { get; set; } = new();
    }

    public class SurveyElementOptionUpdateNestedDto
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
