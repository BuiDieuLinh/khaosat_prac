using System;

namespace khaosat_api.Models
{
    public class SurveyParticipant
    {
        public Guid Id { get; set; }
        public Guid SurveyId { get; set; }
        public Guid? EmployeeId { get; set; }
        public int Status { get; set; } // 0: Pending, 1: Submitted
        public DateTime SubmittedAt { get; set; } = DateTime.Now;
    }
}
