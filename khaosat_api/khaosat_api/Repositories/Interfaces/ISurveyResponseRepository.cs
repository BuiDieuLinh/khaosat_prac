using System.Collections.Generic;
using khaosat_api.Models;

namespace khaosat_api.Repositories.Interfaces
{
    public interface ISurveyResponseRepository
    {
        List<SurveyResponse> GetAll();
        void Add(SurveyResponse response);
        Dictionary<Guid, int> GetCompletedCounts();
        int GetCountBySurveyAndEmployee(Guid surveyId, Guid employeeId);
        int GetCountBySurveyId(Guid surveyId);
    }
}
