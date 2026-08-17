using khaosat_api.Models;
using System.Collections.Generic;

namespace khaosat_api.Repositories.Interfaces
{
    public interface ISurveyAnswerRepository
    {
        List<SurveyAnswer> GetAll();
        List<SurveyAnswer> GetBySurveyId(Guid surveyId);
        void Add(SurveyAnswer answer);
    }
}
