using khaosat_api.DTOs;
using khaosat_api.Models;
using System.Collections.Generic;

namespace khaosat_api.Repositories.Interfaces
{
    public interface ISurveyRepository
    {
        List<Survey> GetAll();
        PagedResult<Survey> GetSurveys(SurveyFilterDto filter, Guid? currentUserId = null);
        Survey? GetById(Guid id);
        void Add(Survey survey);
        void Update(Survey survey);
        void UpdateStatus(Guid id, byte status);
        void UpdateAccessType(Guid id, int accessType);
        void UpdateAnonymousMode(Guid id, bool anonymousMode);
        void DeleteElementsAndOptions(Guid surveyId);
    }
}
