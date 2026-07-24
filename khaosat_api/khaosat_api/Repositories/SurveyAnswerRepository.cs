using khaosat_api.Data;
using khaosat_api.Models;
using khaosat_api.Repositories.Interfaces;
using System.Data.SqlClient;

namespace khaosat_api.Repositories
{
    public class SurveyAnswerRepository : ISurveyAnswerRepository
    {
        private readonly SqlConnectionFactory _factory;

        public SurveyAnswerRepository(SqlConnectionFactory factory)
        {
            _factory = factory;
        }

        public List<SurveyAnswer> GetAll()
        {
            var result = new List<SurveyAnswer>();

            using var conn = _factory.Create();
            conn.Open();

            var cmd = new SqlCommand("SELECT * FROM SurveyAnswer", conn);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new SurveyAnswer
                {
                    Id = Guid.Parse(reader["Id"].ToString()!),
                    ResponseId = Guid.Parse(reader["ResponseId"].ToString()!),
                    ElementId = Guid.Parse(reader["ElementId"].ToString()!),
                    OptionId = reader["OptionId"] == DBNull.Value ? null : (Guid?)Guid.Parse(reader["OptionId"].ToString()!),
                    Value = reader["Value"] == DBNull.Value ? null : reader["Value"].ToString()
                });
            }

            return result;
        }

        public void Add(SurveyAnswer answer)
        {
            using var conn = _factory.Create();
            conn.Open();

            const string sql = @"
                INSERT INTO SurveyAnswer
                (
                    Id,
                    ResponseId,
                    ElementId,
                    OptionId,
                    Value
                )
                VALUES
                (
                    @Id,
                    @ResponseId,
                    @ElementId,
                    @OptionId,
                    @Value
                )";

            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@Id", answer.Id);
            cmd.Parameters.AddWithValue("@ResponseId", answer.ResponseId);
            cmd.Parameters.AddWithValue("@ElementId", answer.ElementId);
            cmd.Parameters.AddWithValue("@OptionId", (object?)answer.OptionId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Value", (object?)answer.Value ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }
    }
}
