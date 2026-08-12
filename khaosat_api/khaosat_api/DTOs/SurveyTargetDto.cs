using System;

namespace khaosat_api.DTOs
{
    public class SurveyTargetDto
    {
        public int TargetType { get; set; } // 1 = Company, 2 = Department, 3 = Position, 4 = Employee
        public Guid? DepartmentId { get; set; }
        public Guid? PositionId { get; set; }
    }
}
