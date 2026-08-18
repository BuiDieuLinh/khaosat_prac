using khaosat_api.Models;

namespace khaosat_api.Repositories.Interfaces
{
    public interface ISurveyParticipantRepository
    {
        void Add(SurveyParticipant participant);
        int GetCountBySurveyAndEmployee(Guid surveyId, Guid employeeId);
        SurveyParticipant? GetBySurveyAndEmployee(Guid surveyId, Guid employeeId);
        void UpdateStatus(Guid surveyId, Guid employeeId, int status);
        List<SurveyParticipant> GetBySurveyId(Guid surveyId);
        Dictionary<Guid, int> GetCompletedCounts();
    }
}
