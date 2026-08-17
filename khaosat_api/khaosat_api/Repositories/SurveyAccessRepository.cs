using khaosat_api.Data;
using khaosat_api.Models;
using khaosat_api.Repositories.Interfaces;
using System;
using System.Data.SqlClient;

namespace khaosat_api.Repositories
{
    public class SurveyAccessRepository : ISurveyAccessRepository
    {
        private readonly SqlConnectionFactory _factory;

        public SurveyAccessRepository(SqlConnectionFactory factory)
        {
            _factory = factory;
        }

        public SurveyAccess? GetBySurveyId(Guid surveyId)
        {
            using var conn = _factory.Create();
            conn.Open();

            const string sql = "SELECT * FROM SurveyAccess WHERE SurveyId = @SurveyId";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@SurveyId", surveyId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return MapFromReader(reader);
            }
            return null;
        }

        public SurveyAccess? GetByTokenHash(string tokenHash)
        {
            using var conn = _factory.Create();
            conn.Open();

            const string sql = "SELECT * FROM SurveyAccess WHERE TokenHash = @TokenHash AND IsActive = 1";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@TokenHash", tokenHash);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return MapFromReader(reader);
            }
            return null;
        }

        public void Add(SurveyAccess access)
        {
            using var conn = _factory.Create();
            conn.Open();

            const string sql = @"
                INSERT INTO SurveyAccess (Id, SurveyId, AccessType, TokenHash, IsActive, StartDate, EndDate)
                VALUES (@Id, @SurveyId, @AccessType, @TokenHash, @IsActive, @StartDate, @EndDate)";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", access.Id == Guid.Empty ? Guid.NewGuid() : access.Id);
            cmd.Parameters.AddWithValue("@SurveyId", access.SurveyId);
            cmd.Parameters.AddWithValue("@AccessType", access.AccessType);
            cmd.Parameters.AddWithValue("@TokenHash", (object?)access.TokenHash ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsActive", access.IsActive);
            cmd.Parameters.AddWithValue("@StartDate", (object?)access.StartDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EndDate", (object?)access.EndDate ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        public void Update(SurveyAccess access)
        {
            using var conn = _factory.Create();
            conn.Open();

            const string sql = @"
                UPDATE SurveyAccess 
                SET AccessType = @AccessType, TokenHash = @TokenHash, IsActive = @IsActive, StartDate = @StartDate, EndDate = @EndDate
                WHERE Id = @Id";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", access.Id);
            cmd.Parameters.AddWithValue("@AccessType", access.AccessType);
            cmd.Parameters.AddWithValue("@TokenHash", (object?)access.TokenHash ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsActive", access.IsActive);
            cmd.Parameters.AddWithValue("@StartDate", (object?)access.StartDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EndDate", (object?)access.EndDate ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        private static SurveyAccess MapFromReader(SqlDataReader reader)
        {
            return new SurveyAccess
            {
                Id = Guid.Parse(reader["Id"].ToString()!),
                SurveyId = Guid.Parse(reader["SurveyId"].ToString()!),
                AccessType = Convert.ToInt32(reader["AccessType"]),
                TokenHash = reader["TokenHash"] == DBNull.Value ? null : reader["TokenHash"].ToString(),
                IsActive = Convert.ToBoolean(reader["IsActive"]),
                StartDate = reader["StartDate"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(reader["StartDate"]),
                EndDate = reader["EndDate"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(reader["EndDate"])
            };
        }
    }
}
