using khaosat_api.DTOs;

namespace khaosat_api.Services.Interfaces
{
    public interface ISurveyService
    {
        List<SurveyDto> GetSurveys(Guid? currentUserId = null);
        SurveyDetailDto? GetSurveyDetail(Guid id, Guid? currentUserId = null);
        void SubmitSurvey(SurveySubmitDto dto);
        void CreateNested(SurveyCreateNestedDto dto);
        void UpdateNested(Guid id, SurveyCreateNestedDto dto);
        SurveyDto CloneSurvey(Guid id);
        void CloseSurvey(Guid id);
    }
}
