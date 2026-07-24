using System;

namespace khaosat_api.Models
{
    public class SurveyResponse
    {
        public Guid Id { get; set; }
        public Guid SurveyId { get; set; }
        public Guid EmployeeId { get; set; }
        public DateTime SubmitDate { get; set; }
        public byte Status { get; set; }
    }
}
