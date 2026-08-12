using khaosat_api.Data;
using khaosat_api.Models;
using khaosat_api.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace khaosat_api.Repositories
{
    public class SurveyTargetRepository : ISurveyTargetRepository
    {
        private readonly SqlConnectionFactory _factory;

        public SurveyTargetRepository(SqlConnectionFactory factory)
        {
            _factory = factory;
        }

        public List<SurveyTarget> GetBySurveyId(Guid surveyId)
        {
            var list = new List<SurveyTarget>();
            using var conn = _factory.Create();
            conn.Open();

            const string sql = "SELECT * FROM SurveyTarget WHERE SurveyId = @SurveyId";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@SurveyId", surveyId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new SurveyTarget
                {
                    Id = Guid.Parse(reader["Id"].ToString()!),
                    SurveyId = Guid.Parse(reader["SurveyId"].ToString()!),
                    TargetType = Convert.ToInt32(reader["TargetType"]),
                    DepartmentId = reader["DepartmentId"] == DBNull.Value ? null : Guid.Parse(reader["DepartmentId"].ToString()!),
                    PositionId = reader["PositionId"] == DBNull.Value ? null : Guid.Parse(reader["PositionId"].ToString()!)
                });
            }

            return list;
        }

        public void Add(SurveyTarget target)
        {
            using var conn = _factory.Create();
            conn.Open();

            const string sql = @"
                INSERT INTO SurveyTarget (Id, SurveyId, TargetType, DepartmentId, PositionId)
                VALUES (@Id, @SurveyId, @TargetType, @DepartmentId, @PositionId)";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", target.Id == Guid.Empty ? Guid.NewGuid() : target.Id);
            cmd.Parameters.AddWithValue("@SurveyId", target.SurveyId);
            cmd.Parameters.AddWithValue("@TargetType", target.TargetType);
            cmd.Parameters.AddWithValue("@DepartmentId", (object?)target.DepartmentId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PositionId", (object?)target.PositionId ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        public void DeleteBySurveyId(Guid surveyId)
        {
            using var conn = _factory.Create();
            conn.Open();

            const string sql = "DELETE FROM SurveyTarget WHERE SurveyId = @SurveyId";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@SurveyId", surveyId);
            cmd.ExecuteNonQuery();
        }
    }
}
