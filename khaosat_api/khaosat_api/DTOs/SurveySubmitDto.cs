using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace khaosat_api.DTOs
{
    public class SurveySubmitDto
    {
        [Required]
        public Guid SurveyId { get; set; }

        public Guid? EmployeeId { get; set; }

        public string? PublicToken { get; set; }
        public string? CookieId { get; set; }

        [Required]
        public List<AnswerSubmitDto> Answers { get; set; } = new();
    }
}
