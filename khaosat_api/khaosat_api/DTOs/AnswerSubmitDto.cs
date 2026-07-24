using System;
using System.ComponentModel.DataAnnotations;

namespace khaosat_api.DTOs
{
    public class AnswerSubmitDto
    {
        [Required]
        public Guid ElementId { get; set; }

        public Guid? OptionId { get; set; }

        public string? Value { get; set; }
    }
}
