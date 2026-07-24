using khaosat_api.Data;
using khaosat_api.Models;
using khaosat_api.Repositories.Interfaces;
using System.Data.SqlClient;

namespace khaosat_api.Repositories
{
    public class SurveyElementRepository : ISurveyElementRepository
    {
        private readonly SqlConnectionFactory _factory;

        public SurveyElementRepository(SqlConnectionFactory factory)
        {
            _factory = factory;
        }

        public List<SurveyElement> GetAll()
        {
            var result = new List<SurveyElement>();

            using var conn = _factory.Create();
            conn.Open();

            var cmd = new SqlCommand("SELECT * FROM SurveyElement", conn);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new SurveyElement
                {
                    Id = Guid.Parse(reader["Id"].ToString()!),
                    SurveyId = Guid.Parse(reader["SurveyId"].ToString()!),
                    FieldName = reader["FieldName"].ToString()!,
                    SortOrder = Convert.ToInt32(reader["SortOrder"]),
                    ConfigType = reader["ConfigType"].ToString()!
                });
            }

            return result;
        }

        public List<SurveyElement> GetBySurveyId(Guid surveyId)
        {
            var result = new List<SurveyElement>();

            using var conn = _factory.Create();
            conn.Open();

            var cmd = new SqlCommand("SELECT * FROM SurveyElement WHERE SurveyId = @SurveyId ORDER BY SortOrder", conn);
            cmd.Parameters.AddWithValue("@SurveyId", surveyId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new SurveyElement
                {
                    Id = Guid.Parse(reader["Id"].ToString()!),
                    SurveyId = Guid.Parse(reader["SurveyId"].ToString()!),
                    FieldName = reader["FieldName"].ToString()!,
                    SortOrder = Convert.ToInt32(reader["SortOrder"]),
                    ConfigType = reader["ConfigType"].ToString()!
                });
            }

            return result;
        }

        public void Add(SurveyElement element)
        {
            using var conn = _factory.Create();
            conn.Open();

            const string sql = @"
                INSERT INTO SurveyElement
                (
                    Id,
                    SurveyId,
                    FieldName,
                    SortOrder,
                    ConfigType
                )
                VALUES
                (
                    @Id,
                    @SurveyId,
                    @FieldName,
                    @SortOrder,
                    @ConfigType
                )";

            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@Id", element.Id);
            cmd.Parameters.AddWithValue("@SurveyId", element.SurveyId);
            cmd.Parameters.AddWithValue("@FieldName", element.FieldName);
            cmd.Parameters.AddWithValue("@SortOrder", element.SortOrder);
            cmd.Parameters.AddWithValue("@ConfigType", element.ConfigType);

            cmd.ExecuteNonQuery();
        }
    }
}
