using khaosat_api.Models;
using System;

namespace khaosat_api.Repositories.Interfaces
{
    public interface ISurveyAccessRepository
    {
        SurveyAccess? GetBySurveyId(Guid surveyId);
        SurveyAccess? GetByTokenHash(string tokenHash);
        void Add(SurveyAccess access);
        void Update(SurveyAccess access);
    }
}
