using khaosat_api.DTOs;

namespace khaosat_api.Services.Interfaces
{
    public interface ISurveyService
    {
        List<SurveyDto> GetSurveys();
        SurveyDetailDto? GetSurveyDetail(Guid id);
        void SubmitSurvey(SurveySubmitDto dto);
        void CreateNested(SurveyCreateNestedDto dto);
    }
}
