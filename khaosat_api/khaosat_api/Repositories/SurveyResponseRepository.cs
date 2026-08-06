using khaosat_api.Data;
using khaosat_api.Models;
using khaosat_api.Repositories.Interfaces;
using System.Data.SqlClient;

namespace khaosat_api.Repositories
{
    public class SurveyResponseRepository : ISurveyResponseRepository
    {
        private readonly SqlConnectionFactory _factory;

        public SurveyResponseRepository(SqlConnectionFactory factory)
        {
            _factory = factory;
        }

        public List<SurveyResponse> GetAll()
        {
            var result = new List<SurveyResponse>();

            using var conn = _factory.Create();
            conn.Open();

            var cmd = new SqlCommand("SELECT * FROM SurveyResponse", conn);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new SurveyResponse
                {
                    Id = Guid.Parse(reader["Id"].ToString()!),
                    SurveyId = Guid.Parse(reader["SurveyId"].ToString()!),
                    EmployeeId = Guid.Parse(reader["EmployeeId"].ToString()!),
                    SubmitDate = Convert.ToDateTime(reader["SubmitDate"]),
                    Status = Convert.ToByte(reader["Status"])
                });
            }

            return result;
        }

        public void Add(SurveyResponse response)
        {
            using var conn = _factory.Create();
            conn.Open();

            const string sql = @"
                INSERT INTO SurveyResponse
                (
                    Id,
                    SurveyId,
                    EmployeeId,
                    SubmitDate,
                    Status
                )
                VALUES
                (
                    @Id,
                    @SurveyId,
                    @EmployeeId,
                    @SubmitDate,
                    @Status
                )";

            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@Id", response.Id);
            cmd.Parameters.AddWithValue("@SurveyId", response.SurveyId);
            cmd.Parameters.AddWithValue("@EmployeeId", response.EmployeeId);
            cmd.Parameters.AddWithValue("@SubmitDate", response.SubmitDate);
            cmd.Parameters.AddWithValue("@Status", response.Status);

            cmd.ExecuteNonQuery();
        }

        public Dictionary<Guid, int> GetCompletedCounts()
        {
            var result = new Dictionary<Guid, int>();

            using var conn = _factory.Create();
            conn.Open();

            const string sql = "SELECT SurveyId, COUNT(DISTINCT EmployeeId) AS CompletedCount FROM SurveyResponse GROUP BY SurveyId";
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

        public int GetCountBySurveyAndEmployee(Guid surveyId, Guid employeeId)
        {
            using var conn = _factory.Create();
            conn.Open();

            const string sql = "SELECT COUNT(*) FROM SurveyResponse WHERE SurveyId = @SurveyId AND EmployeeId = @EmployeeId";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@SurveyId", surveyId);
            cmd.Parameters.AddWithValue("@EmployeeId", employeeId);

            var count = cmd.ExecuteScalar();
            return count != null && count != DBNull.Value ? Convert.ToInt32(count) : 0;
        }

        public int GetCountBySurveyId(Guid surveyId)
        {
            using var conn = _factory.Create();
            conn.Open();

            const string sql = "SELECT COUNT(*) FROM SurveyResponse WHERE SurveyId = @SurveyId";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@SurveyId", surveyId);

            var count = cmd.ExecuteScalar();
            return count != null && count != DBNull.Value ? Convert.ToInt32(count) : 0;
        }
    }
}
