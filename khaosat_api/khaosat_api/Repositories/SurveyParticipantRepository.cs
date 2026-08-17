using khaosat_api.Data;
using khaosat_api.Models;
using khaosat_api.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace khaosat_api.Repositories
{
    public class SurveyParticipantRepository : ISurveyParticipantRepository
    {
        private readonly SqlConnectionFactory _factory;

        public SurveyParticipantRepository(SqlConnectionFactory factory)
        {
            _factory = factory;
        }

        public void Add(SurveyParticipant participant)
        {
            using var conn = _factory.Create();
            conn.Open();

            const string sql = @"
                INSERT INTO SurveyParticipant (Id, SurveyId, EmployeeId, Status, SubmittedAt)
                VALUES (@Id, @SurveyId, @EmployeeId, @Status, @SubmittedAt)";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", participant.Id == Guid.Empty ? Guid.NewGuid() : participant.Id);
            cmd.Parameters.AddWithValue("@SurveyId", participant.SurveyId);
            cmd.Parameters.AddWithValue("@EmployeeId", (object?)participant.EmployeeId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Status", participant.Status);
            cmd.Parameters.AddWithValue("@SubmittedAt", participant.SubmittedAt);

            cmd.ExecuteNonQuery();
        }

        public int GetCountBySurveyAndEmployee(Guid surveyId, Guid employeeId)
        {
            using var conn = _factory.Create();
            conn.Open();

            const string sql = "SELECT COUNT(*) FROM SurveyParticipant WHERE SurveyId = @SurveyId AND EmployeeId = @EmployeeId AND Status = 1";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@SurveyId", surveyId);
            cmd.Parameters.AddWithValue("@EmployeeId", employeeId);

            var count = cmd.ExecuteScalar();
            return count != null && count != DBNull.Value ? Convert.ToInt32(count) : 0;
        }

        public SurveyParticipant? GetBySurveyAndEmployee(Guid surveyId, Guid employeeId)
        {
            using var conn = _factory.Create();
            conn.Open();

            const string sql = "SELECT * FROM SurveyParticipant WHERE SurveyId = @SurveyId AND EmployeeId = @EmployeeId";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@SurveyId", surveyId);
            cmd.Parameters.AddWithValue("@EmployeeId", employeeId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new SurveyParticipant
                {
                    Id = Guid.Parse(reader["Id"].ToString()!),
                    SurveyId = Guid.Parse(reader["SurveyId"].ToString()!),
                    EmployeeId = reader["EmployeeId"] == DBNull.Value ? null : Guid.Parse(reader["EmployeeId"].ToString()!),
                    Status = Convert.ToInt32(reader["Status"]),
                    SubmittedAt = Convert.ToDateTime(reader["SubmittedAt"])
                };
            }

            return null;
        }

        public void UpdateStatus(Guid surveyId, Guid employeeId, int status)
        {
            using var conn = _factory.Create();
            conn.Open();

            const string sql = "UPDATE SurveyParticipant SET Status = @Status, SubmittedAt = @SubmittedAt WHERE SurveyId = @SurveyId AND EmployeeId = @EmployeeId";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@SurveyId", surveyId);
            cmd.Parameters.AddWithValue("@EmployeeId", employeeId);
            cmd.Parameters.AddWithValue("@Status", status);
            cmd.Parameters.AddWithValue("@SubmittedAt", DateTime.Now);

            cmd.ExecuteNonQuery();
        }

        public List<SurveyParticipant> GetBySurveyId(Guid surveyId)
        {
            var result = new List<SurveyParticipant>();
            using var conn = _factory.Create();
            conn.Open();

            const string sql = "SELECT * FROM SurveyParticipant WHERE SurveyId = @SurveyId";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@SurveyId", surveyId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new SurveyParticipant
                {
                    Id = Guid.Parse(reader["Id"].ToString()!),
                    SurveyId = Guid.Parse(reader["SurveyId"].ToString()!),
                    EmployeeId = reader["EmployeeId"] == DBNull.Value ? null : Guid.Parse(reader["EmployeeId"].ToString()!),
                    Status = Convert.ToInt32(reader["Status"]),
                    SubmittedAt = Convert.ToDateTime(reader["SubmittedAt"])
                });
            }

            return result;
        }

        public Dictionary<Guid, int> GetCompletedCounts()
        {
            var result = new Dictionary<Guid, int>();

            using var conn = _factory.Create();
            conn.Open();

            const string sql = "SELECT SurveyId, COUNT(DISTINCT EmployeeId) AS CompletedCount FROM SurveyParticipant WHERE Status = 1 AND EmployeeId IS NOT NULL GROUP BY SurveyId";
            using var cmd = new SqlCommand(sql, conn);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var surveyId = Guid.Parse(reader["SurveyId"].ToString()!);
                var count = Convert.ToInt32(reader["CompletedCount"]);
                result[surveyId] = count;
            }

            return result;
        }
    }
}
