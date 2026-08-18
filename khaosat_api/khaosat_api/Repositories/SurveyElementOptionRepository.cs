using System.Data.SqlClient;
using khaosat_api.Data;
using khaosat_api.Models;
using khaosat_api.Repositories.Interfaces;

namespace khaosat_api.Repositories
{
    public class SurveyElementOptionRepository : ISurveyElementOptionRepository
    {
        private readonly SqlConnectionFactory _factory;

        public SurveyElementOptionRepository(SqlConnectionFactory factory)
        {
            _factory = factory;
        }

        public List<SurveyElementOption> GetAll()
        {
            var result = new List<SurveyElementOption>();

            using var conn = _factory.Create();
            conn.Open();

            var cmd = new SqlCommand("SELECT * FROM SurveyElementOption", conn);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new SurveyElementOption
                {
                    Id = Guid.Parse(reader["Id"].ToString()!),
                    ElementId = Guid.Parse(reader["ElementId"].ToString()!),
                    Value = reader["Value"].ToString()!,
                    DisplayText = reader["DisplayText"].ToString()!,
                    SortOrder = Convert.ToInt32(reader["SortOrder"]),
                    IsDefault = Convert.ToBoolean(reader["IsDefault"]),
                    IsActive = Convert.ToBoolean(reader["IsActive"])
                });
            }

            return result;
        }

        public List<SurveyElementOption> GetByElementId(Guid elementId)
        {
            var result = new List<SurveyElementOption>();

            using var conn = _factory.Create();
            conn.Open();

            var cmd = new SqlCommand("SELECT * FROM SurveyElementOption WHERE ElementId = @ElementId AND IsActive = 1 ORDER BY SortOrder", conn);
            cmd.Parameters.AddWithValue("@ElementId", elementId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new SurveyElementOption
                {
                    Id = Guid.Parse(reader["Id"].ToString()!),
                    ElementId = Guid.Parse(reader["ElementId"].ToString()!),
                    Value = reader["Value"].ToString()!,
                    DisplayText = reader["DisplayText"].ToString()!,
                    SortOrder = Convert.ToInt32(reader["SortOrder"]),
                    IsDefault = Convert.ToBoolean(reader["IsDefault"]),
                    IsActive = Convert.ToBoolean(reader["IsActive"])
                });
            }

            return result;
        }

        public void Add(SurveyElementOption option)
        {
            using var conn = _factory.Create();
            conn.Open();

            const string sql = @"
                INSERT INTO SurveyElementOption
                (
                    Id,
                    ElementId,
                    Value,
                    DisplayText,
                    SortOrder,
                    IsDefault,
                    IsActive
                )
                VALUES
                (
                    @Id,
                    @ElementId,
                    @Value,
                    @DisplayText,
                    @SortOrder,
                    @IsDefault,
                    @IsActive
                )";

            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@Id", option.Id);
            cmd.Parameters.AddWithValue("@ElementId", option.ElementId);
            cmd.Parameters.AddWithValue("@Value", option.Value);
            cmd.Parameters.AddWithValue("@DisplayText", option.DisplayText);
            cmd.Parameters.AddWithValue("@SortOrder", option.SortOrder);
            cmd.Parameters.AddWithValue("@IsDefault", option.IsDefault);
            cmd.Parameters.AddWithValue("@IsActive", option.IsActive);

            cmd.ExecuteNonQuery();
        }
    }
}
