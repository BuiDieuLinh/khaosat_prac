using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace khaosat_api.DTOs
{
    public class SurveySubmitDto
    {
        [Required]
        public Guid SurveyId { get; set; }

        [Required]
        public Guid EmployeeId { get; set; }

        [Required]
        public List<AnswerSubmitDto> Answers { get; set; } = new();
    }
}
