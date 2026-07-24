using khaosat_api.Models;
using System.Collections.Generic;

namespace khaosat_api.Repositories.Interfaces
{
    public interface ISurveyResponseRepository
    {
        List<SurveyResponse> GetAll();
        void Add(SurveyResponse response);
        Dictionary<Guid, int> GetCompletedCounts();
    }
}
