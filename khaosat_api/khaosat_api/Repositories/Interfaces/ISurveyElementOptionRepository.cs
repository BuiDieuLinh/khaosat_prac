using System.Collections.Generic;
using khaosat_api.Models;

namespace khaosat_api.Repositories.Interfaces
{
    public interface ISurveyElementOptionRepository
    {
        List<SurveyElementOption> GetAll();
        List<SurveyElementOption> GetByElementId(Guid elementId);
        void Add(SurveyElementOption option);
    }
}
