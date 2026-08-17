using System;
using System.Collections.Generic;

namespace khaosat_api.DTOs
{
    public class SurveyReportDto
    {
        public Guid SurveyId { get; set; }
        public string SurveyCode { get; set; } = string.Empty;
        public string SurveyName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public byte Status { get; set; }
        public int AccessType { get; set; }
        public bool AnonymousMode { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedDate { get; set; }

        public int TotalResponses { get; set; }
        public int TotalTargetParticipants { get; set; }
        public int CompletedParticipantsCount { get; set; }
        public double CompletionRate { get; set; }
        public string TargetSummary { get; set; } = string.Empty;

        public List<QuestionReportDto> Questions { get; set; } = new();
        public List<DepartmentReportDto> DepartmentBreakdown { get; set; } = new();
        public List<GroupReportItemDto> GroupBreakdown { get; set; } = new();
    }

    public class QuestionReportDto
    {
        public Guid ElementId { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string Caption { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public int TotalAnswerCount { get; set; }

        public List<OptionReportDto> Options { get; set; } = new();
        public List<string> TextAnswers { get; set; } = new();
    }

    public class OptionReportDto
    {
        public Guid OptionId { get; set; }
        public string DisplayText { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class DepartmentReportDto
    {
        public Guid? DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int TotalAssigned { get; set; }
        public int CompletedCount { get; set; }
        public double CompletionRate { get; set; }
    }

    public class GroupReportItemDto
    {
        public string GroupName { get; set; } = string.Empty;
        public int ResponseCount { get; set; }
        public bool IsPrivacyThresholdMet { get; set; }
        public string? Note { get; set; }
    }
}
