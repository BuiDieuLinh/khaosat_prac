using System.Collections.Generic;
using khaosat_api.Models;

namespace khaosat_api.Repositories.Interfaces
{
    public interface ISurveyAnswerRepository
    {
        List<SurveyAnswer> GetAll();
        List<SurveyAnswer> GetBySurveyId(Guid surveyId);
        void Add(SurveyAnswer answer);
    }
}
