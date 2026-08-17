using System;
using System.Collections.Generic;

namespace khaosat_api.DTOs
{
    public class SurveyDetailDto
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
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }    
        public List<SurveyTargetDto> Targets { get; set; } = new();
        public List<SurveyElementDetailDto> Elements { get; set; } = new();
    }
}
