using System;
using System.ComponentModel.DataAnnotations;

namespace khaosat_api.Models
{
    public class Survey
    {
        public Guid Id { get; set; }

        [Display(Name = "Mã khảo sát")]
        [Required(ErrorMessage = "Mã khảo sát không được để trống")]
        [StringLength(8, MinimumLength = 2, ErrorMessage = "Độ dài phải từ 2 đến 8 ký tự")]
        public string Code { get; set; } = string.Empty;

        [Display(Name = "Tên khảo sát")]
        [Required(ErrorMessage = "Tên khảo sát không được để trống")]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (StartDate.HasValue && EndDate.HasValue &&
                EndDate <= StartDate)
            {
                yield return new ValidationResult(
                    "Ngày kết thúc phải lớn hơn ngày bắt đầu.",
                    new[] { nameof(EndDate) });
            }
        }
        public SurveyStatus Status { get; set; }
        public int? MaxAttempts { get; set; }
        public int AccessType { get; set; } = 1; // 1: Internal, 2: Public, 3: Invitation
        public bool AnonymousMode { get; set; } = false;
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    public enum SurveyStatus : byte
    {
        Draft = 0,
        Active = 1,
        Closed = 2
    }
}
