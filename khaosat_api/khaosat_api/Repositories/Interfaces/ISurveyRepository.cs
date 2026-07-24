using khaosat_api.Models;
using System.Collections.Generic;

namespace khaosat_api.Repositories.Interfaces
{
    public interface ISurveyRepository
    {
        List<Survey> GetAll();
        Survey? GetById(Guid id);
        void Add(Survey survey);
        void UpdateStatus(Guid id, byte status);
    }
}
