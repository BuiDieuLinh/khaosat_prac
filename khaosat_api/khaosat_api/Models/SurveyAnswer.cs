using System;

namespace khaosat_api.Models
{
    public class SurveyAnswer
    {
        public Guid Id { get; set; }
        public Guid ResponseId { get; set; }
        public Guid ElementId { get; set; }
        public Guid? OptionId { get; set; }
        public string? Value { get; set; }
    }
}
