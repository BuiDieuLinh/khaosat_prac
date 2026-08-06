using System;

namespace khaosat_api.Models
{
    public class SurveyTarget
    {
        public Guid Id { get; set; }
        public Guid SurveyId { get; set; }
        public int TargetType { get; set; } // 1 = Company, 2 = Department, 3 = Position, 4 = Employee
        public Guid? TargetId { get; set; }
    }
}
