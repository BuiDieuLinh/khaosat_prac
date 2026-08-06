using khaosat_api.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace khaosat_api.Repositories.Interfaces
{
    public interface ISurveyTargetRepository
    {
        List<SurveyTarget> GetBySurveyId(Guid surveyId);
        void Add(SurveyTarget target);
        void DeleteBySurveyId(Guid surveyId);
    }
}
