using khaosat_api.Models;
using System.Collections.Generic;

namespace khaosat_api.Repositories.Interfaces
{
    public interface ISurveyElementRepository
    {
        List<SurveyElement> GetAll();
        List<SurveyElement> GetBySurveyId(Guid surveyId);
        void Add(SurveyElement element);
    }
}
