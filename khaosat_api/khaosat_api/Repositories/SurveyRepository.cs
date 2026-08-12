using System.Data.SqlClient;
using khaosat_api.Data;
using khaosat_api.DTOs;
using khaosat_api.Models;
using khaosat_api.Repositories.Interfaces;

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
                    Status = (SurveyStatus)Convert.ToByte(reader["Status"]),
                    MaxAttempts = HasColumn(reader, "MaxAttempts") && reader["MaxAttempts"] != DBNull.Value ? Convert.ToInt32(reader["MaxAttempts"]) : null,
                    CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                    UpdatedDate = reader["UpdatedDate"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(reader["UpdatedDate"])
                });
            }

            return result;
        }

        private void AddParameters(SqlCommand cmd, SurveyFilterDto filter, Guid? currentUserId)
        {
            if (!string.IsNullOrWhiteSpace(filter.SearchKeyword))
                cmd.Parameters.AddWithValue("@Search", $"%{filter.SearchKeyword.Trim()}%");

            if (filter.Status.HasValue)
                cmd.Parameters.AddWithValue("@Status", (byte)filter.Status.Value);

            if (currentUserId.HasValue && currentUserId.Value != Guid.Empty)
                cmd.Parameters.AddWithValue("@CurrentUserId", currentUserId.Value);
        }

        public PagedResult<Survey> GetSurveys(SurveyFilterDto filter, Guid? currentUserId)
        {
            int pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
            int pageSize = filter.PageSize < 1 ? 10 : Math.Min(filter.PageSize, 100);
            int skip = (pageNumber - 1) * pageSize;

            using var conn = _factory.Create();
            conn.Open();

            var whereConditions = new List<string>();

            if (!string.IsNullOrWhiteSpace(filter.SearchKeyword))
                whereConditions.Add("(s.Code LIKE @Search OR s.Name LIKE @Search OR s.Description LIKE @Search)");

            if (filter.Status.HasValue)
                whereConditions.Add("s.Status = @Status");

            if (currentUserId.HasValue && currentUserId.Value != Guid.Empty)
            {
                whereConditions.Add(@"
                (
                    NOT EXISTS (
                        SELECT 1
                        FROM SurveyTarget st
                        WHERE st.SurveyId = s.Id
                    )
                    OR
                    EXISTS (
                        SELECT 1
                        FROM Employee e
                        LEFT JOIN Position p ON p.Id = e.PositionId
                        WHERE e.Id = @CurrentUserId
                        AND EXISTS (
                            SELECT 1
                            FROM SurveyTarget st
                            WHERE st.SurveyId = s.Id
                            AND (
                                -- Toàn công ty
                                (
                                    st.DepartmentId IS NULL
                                    AND st.PositionId IS NULL
                                )
                                OR
                                -- Toàn bộ Position của Department
                                (
                                    st.DepartmentId = p.DepartmentId
                                    AND st.PositionId IS NULL
                                )
                                OR
                                -- Position cụ thể
                                (
                                    st.DepartmentId = p.DepartmentId
                                    AND st.PositionId = e.PositionId
                                )
                            )
                        )
                    )
                )");
            }

            string whereSql = whereConditions.Count > 0
                ? "WHERE " + string.Join(" AND ", whereConditions)
                : "";

            string countSql = $@"SELECT COUNT(1) FROM Survey s {whereSql}";

            using var countCmd = new SqlCommand(countSql, conn);
            AddParameters(countCmd, filter, currentUserId);

            int totalCount = Convert.ToInt32(countCmd.ExecuteScalar());

            if (totalCount == 0)
                return new PagedResult<Survey>(new List<Survey>(), 0, pageNumber, pageSize);

            string sortColumn = filter.SortBy?.ToLower() switch
            {
                "code" => "s.Code",
                "name" => "s.Name",
                "startdate" => "s.StartDate",
                "enddate" => "s.EndDate",
                "status" => "s.Status",
                "maxattempts" => "s.MaxAttempts",
                "createddate" => "s.CreatedDate",
                _ => "s.CreatedDate"
            };

            string sortDirection = filter.IsDescending ? "DESC" : "ASC";

            string querySql = $@"
                SELECT s.Id, s.Code, s.Name, s.Description, s.StartDate, s.EndDate,
                       s.Status, s.MaxAttempts, s.CreatedDate, s.UpdatedDate
                FROM Survey s
                {whereSql}
                ORDER BY {sortColumn} {sortDirection}, s.Id ASC
                OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY";

            using var queryCmd = new SqlCommand(querySql, conn);
            AddParameters(queryCmd, filter, currentUserId);
            queryCmd.Parameters.AddWithValue("@Skip", skip);
            queryCmd.Parameters.AddWithValue("@Take", pageSize);

            using var reader = queryCmd.ExecuteReader();
            var surveys = new List<Survey>();

            while (reader.Read())
            {
                surveys.Add(new Survey
                {
                    Id = (Guid)reader["Id"],
                    Code = reader["Code"].ToString()!,
                    Name = reader["Name"].ToString()!,
                    Description = reader["Description"] == DBNull.Value ? null : reader["Description"].ToString(),
                    StartDate = reader["StartDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["StartDate"]),
                    EndDate = reader["EndDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["EndDate"]),
                    Status = (SurveyStatus)Convert.ToByte(reader["Status"]),
                    MaxAttempts = reader["MaxAttempts"] == DBNull.Value ? null : Convert.ToInt32(reader["MaxAttempts"]),
                    CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                    UpdatedDate = reader["UpdatedDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["UpdatedDate"])
                });
            }

            return new PagedResult<Survey>(surveys, totalCount, pageNumber, pageSize);
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
                    Status = (SurveyStatus)Convert.ToByte(reader["Status"]),
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
