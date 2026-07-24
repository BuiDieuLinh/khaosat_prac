using System;

namespace khaosat_api.DTOs
{
    public class SurveyElementOptionDto
    {
        public Guid Id { get; set; }
        public Guid ElementId { get; set; }
        public string Value { get; set; } = string.Empty;
        public string DisplayText { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
    }
}
