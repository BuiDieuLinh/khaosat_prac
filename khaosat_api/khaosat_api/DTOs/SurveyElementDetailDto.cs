using System;
using System.Collections.Generic;

namespace khaosat_api.DTOs
{
    public class SurveyElementDetailDto
    {
        public Guid Id { get; set; }
        public Guid SurveyId { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public int SortOrder { get; set; }

        public string ConfigType { get; set; } = string.Empty;
        public List<SurveyElementOptionDto> Options { get; set; } = new();
    }
}
