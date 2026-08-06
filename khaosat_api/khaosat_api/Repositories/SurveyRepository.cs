using khaosat_api.Data;
using khaosat_api.Models;
using khaosat_api.Repositories.Interfaces;
using System.Data.SqlClient;

namespace khaosat_api.Repositories
{
    public class SurveyRepository : ISurveyRepository
    {
        private readonly SqlConnectionFactory _factory;

        public SurveyRepository(SqlConnectionFactory factory)
        {
            _factory = factory;
        }

        private static bool HasColumn(SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        public List<Survey> GetAll()
        {
            var result = new List<Survey>();

            using var conn = _factory.Create();
            conn.Open();

            var cmd = new SqlCommand("SELECT * FROM Survey ORDER BY CreatedDate DESC", conn);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new Survey
                {
                    Id = Guid.Parse(reader["Id"].ToString()!),
                    Code = reader["Code"].ToString()!,
                    Name = reader["Name"].ToString()!,
                    Description = reader["Description"] == DBNull.Value ? null : reader["Description"].ToString(),
                    StartDate = reader["StartDate"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(reader["StartDate"]),
                    EndDate = reader["EndDate"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(reader["EndDate"]),
                    Status = Convert.ToByte(reader["Status"]),
                    MaxAttempts = HasColumn(reader, "MaxAttempts") && reader["MaxAttempts"] != DBNull.Value ? Convert.ToInt32(reader["MaxAttempts"]) : null,
                    CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                    UpdatedDate = reader["UpdatedDate"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(reader["UpdatedDate"])
                });
            }

            return result;
        }

        public Survey? GetById(Guid id)
        {
            using var conn = _factory.Create();
            conn.Open();

            var cmd = new SqlCommand("SELECT * FROM Survey WHERE Id = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Survey
                {
                    Id = Guid.Parse(reader["Id"].ToString()!),
                    Code = reader["Code"].ToString()!,
                    Name = reader["Name"].ToString()!,
                    Description = reader["Description"] == DBNull.Value ? null : reader["Description"].ToString(),
                    StartDate = reader["StartDate"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(reader["StartDate"]),
                    EndDate = reader["EndDate"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(reader["EndDate"]),
                    Status = Convert.ToByte(reader["Status"]),
                    MaxAttempts = HasColumn(reader, "MaxAttempts") && reader["MaxAttempts"] != DBNull.Value ? Convert.ToInt32(reader["MaxAttempts"]) : null,
                    CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                    UpdatedDate = reader["UpdatedDate"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(reader["UpdatedDate"])
                };
            }

            return null;
        }

        public void Add(Survey survey)
        {
            using var conn = _factory.Create();
            conn.Open();

            const string sql = @"
                INSERT INTO Survey
                (
                    Id, Code, Name, Description, StartDate, EndDate, Status, MaxAttempts, CreatedDate, UpdatedDate
                )
                VALUES
                (
                    @Id, @Code, @Name, @Description, @StartDate, @EndDate, @Status, @MaxAttempts, @CreatedDate, @UpdatedDate
                )";

            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@Id", survey.Id);
            cmd.Parameters.AddWithValue("@Code", survey.Code);
            cmd.Parameters.AddWithValue("@Name", survey.Name);
            cmd.Parameters.AddWithValue("@Description", (object?)survey.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@StartDate", (object?)survey.StartDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EndDate", (object?)survey.EndDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Status", survey.Status);
            cmd.Parameters.AddWithValue("@MaxAttempts", (object?)survey.MaxAttempts ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CreatedDate", survey.CreatedDate);
            cmd.Parameters.AddWithValue("@UpdatedDate", (object?)survey.UpdatedDate ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        public void UpdateStatus(Guid id, byte status)
        {
            using var conn = _factory.Create();
            conn.Open();

            const string sql = "UPDATE Survey SET Status = @Status, UpdatedDate = @UpdatedDate WHERE Id = @Id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@Status", status);
            cmd.Parameters.AddWithValue("@UpdatedDate", DateTime.Now);

            cmd.ExecuteNonQuery();
        }

        public void Update(Survey survey)
        {
            using var conn = _factory.Create();
            conn.Open();

            const string sql = @"
                UPDATE Survey
                SET Code = @Code,
                    Name = @Name,
                    Description = @Description,
                    StartDate = @StartDate,
                    EndDate = @EndDate,
                    Status = @Status,
                    MaxAttempts = @MaxAttempts,
                    UpdatedDate = @UpdatedDate
                WHERE Id = @Id";

            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@Id", survey.Id);
            cmd.Parameters.AddWithValue("@Code", survey.Code);
            cmd.Parameters.AddWithValue("@Name", survey.Name);
            cmd.Parameters.AddWithValue("@Description", (object?)survey.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@StartDate", (object?)survey.StartDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EndDate", (object?)survey.EndDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Status", survey.Status);
            cmd.Parameters.AddWithValue("@MaxAttempts", (object?)survey.MaxAttempts ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@UpdatedDate", (object?)survey.UpdatedDate ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        public void DeleteElementsAndOptions(Guid surveyId)
        {
            using var conn = _factory.Create();
            conn.Open();

            const string sqlDeleteOptions = @"
                DELETE FROM SurveyElementOption 
                WHERE ElementId IN (SELECT Id FROM SurveyElement WHERE SurveyId = @SurveyId)";
            using (var cmd = new SqlCommand(sqlDeleteOptions, conn))
            {
                cmd.Parameters.AddWithValue("@SurveyId", surveyId);
                cmd.ExecuteNonQuery();
            }

            const string sqlDeleteElements = @"
                DELETE FROM SurveyElement 
                WHERE SurveyId = @SurveyId";
            using (var cmd = new SqlCommand(sqlDeleteElements, conn))
            {
                cmd.Parameters.AddWithValue("@SurveyId", surveyId);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
