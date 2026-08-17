using System;

namespace khaosat_api.Models
{
    public class SurveyAccess
    {
        public Guid Id { get; set; }
        public Guid SurveyId { get; set; }
        public int AccessType { get; set; } // 1: Internal, 2: Public, 3: Invitation
        public string? TokenHash { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
