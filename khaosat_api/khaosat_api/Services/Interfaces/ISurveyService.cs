using khaosat_api.DTOs;

namespace khaosat_api.Services.Interfaces
{
    public interface ISurveyService
    {
        List<SurveyDto> GetSurveys(Guid? currentUserId = null);
        PagedResult<SurveyDto> GetSurveys(SurveyFilterDto filter, Guid? currentUserId = null, bool isAdminOrManager = false);
        SurveyDetailDto? GetSurveyDetail(Guid id, Guid? currentUserId = null);
        SurveyDetailDto? GetPublicSurveyDetail(string token);
        void SubmitSurvey(SurveySubmitDto dto, string? username = null, string? ipAddress = null, string? userAgent = null);
        void SubmitPublicSurvey(SurveySubmitDto dto, string? ipAddress = null, string? userAgent = null);
        void CreateNested(SurveyCreateNestedDto dto);
        void UpdateNested(Guid id, SurveyUpdateNestedDto dto);
        SurveyDto CloneSurvey(Guid id);
        void CloseSurvey(Guid id);
        void ChangeAccessType(Guid id, int accessType);
        void ChangeAnonymousMode(Guid id, bool anonymousMode);
        SurveyReportDto GetSurveyReport(Guid id);
        PagedResult<AuditLogDto> GetAuditLogs(int pageNumber, int pageSize, string? actionFilter = null, string? searchKeyword = null);
    }
}
