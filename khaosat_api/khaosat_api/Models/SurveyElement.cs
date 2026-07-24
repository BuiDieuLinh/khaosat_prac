using System;

namespace khaosat_api.Models
{
    public class SurveyElement
    {
        public Guid Id { get; set; }
        public Guid SurveyId { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public int SortOrder { get; set; }

        public string ConfigType { get; set; } = string.Empty;
    }
}
