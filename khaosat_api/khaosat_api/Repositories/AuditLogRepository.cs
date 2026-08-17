using khaosat_api.Data;
using khaosat_api.DTOs;
using khaosat_api.Models;
using khaosat_api.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace khaosat_api.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly SqlConnectionFactory _factory;

        public AuditLogRepository(SqlConnectionFactory factory)
        {
            _factory = factory;
        }

        public void Add(AuditLog log)
        {
            using var conn = _factory.Create();
            conn.Open();

            object entityIdValue = DBNull.Value;
            string entityTypeVal = log.EntityType ?? "Survey";
            if (!string.IsNullOrEmpty(log.EntityId))
            {
                if (int.TryParse(log.EntityId, out int intId))
                {
                    entityIdValue = intId;
                }
                else
                {
                    entityIdValue = DBNull.Value;
                    if (!entityTypeVal.Contains(log.EntityId))
                    {
                        entityTypeVal = $"{entityTypeVal} ({log.EntityId})";
                    }
                }
            }

            const string sql = @"
                INSERT INTO AuditLog (Id, UserName, Action, EntityType, EntityId, OldValue, NewValue, IpAddress, UserAgent, CreatedAt)
                VALUES (@Id, @UserName, @Action, @EntityType, @EntityId, @OldValue, @NewValue, @IpAddress, @UserAgent, @CreatedAt)";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", log.Id == Guid.Empty ? Guid.NewGuid() : log.Id);
            cmd.Parameters.AddWithValue("@UserName", (object?)log.UserName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Action", log.Action);
            cmd.Parameters.AddWithValue("@EntityType", entityTypeVal);
            cmd.Parameters.AddWithValue("@EntityId", entityIdValue);
            cmd.Parameters.AddWithValue("@OldValue", (object?)log.OldValue ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@NewValue", (object?)log.NewValue ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IpAddress", (object?)log.IpAddress ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@UserAgent", (object?)log.UserAgent ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CreatedAt", log.CreatedAt);

            cmd.ExecuteNonQuery();
        }

        public PagedResult<AuditLog> GetLogs(int pageNumber, int pageSize, string? actionFilter = null, string? searchKeyword = null)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, 100);
            int skip = (pageNumber - 1) * pageSize;

            using var conn = _factory.Create();
            conn.Open();

            var whereConditions = new List<string>();
            if (!string.IsNullOrWhiteSpace(actionFilter))
            {
                whereConditions.Add("Action = @ActionFilter");
            }
            if (!string.IsNullOrWhiteSpace(searchKeyword))
            {
                whereConditions.Add("(UserName LIKE @Search OR Action LIKE @Search OR EntityType LIKE @Search OR OldValue LIKE @Search OR NewValue LIKE @Search)");
            }

            string whereSql = whereConditions.Count > 0 ? "WHERE " + string.Join(" AND ", whereConditions) : "";
            string countSql = $"SELECT COUNT(1) FROM AuditLog {whereSql}";

            using var countCmd = new SqlCommand(countSql, conn);
            if (!string.IsNullOrWhiteSpace(actionFilter))
                countCmd.Parameters.AddWithValue("@ActionFilter", actionFilter.Trim());
            if (!string.IsNullOrWhiteSpace(searchKeyword))
                countCmd.Parameters.AddWithValue("@Search", $"%{searchKeyword.Trim()}%");

            int totalCount = Convert.ToInt32(countCmd.ExecuteScalar());
            if (totalCount == 0)
            {
                return new PagedResult<AuditLog>(new List<AuditLog>(), 0, pageNumber, pageSize);
            }

            string querySql = $@"
                SELECT Id, UserName, Action, EntityType, EntityId, OldValue, NewValue, IpAddress, UserAgent, CreatedAt
                FROM AuditLog
                {whereSql}
                ORDER BY CreatedAt DESC
                OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY";

            using var queryCmd = new SqlCommand(querySql, conn);
            if (!string.IsNullOrWhiteSpace(actionFilter))
                queryCmd.Parameters.AddWithValue("@ActionFilter", actionFilter.Trim());
            if (!string.IsNullOrWhiteSpace(searchKeyword))
                queryCmd.Parameters.AddWithValue("@Search", $"%{searchKeyword.Trim()}%");

            queryCmd.Parameters.AddWithValue("@Skip", skip);
            queryCmd.Parameters.AddWithValue("@Take", pageSize);

            using var reader = queryCmd.ExecuteReader();
            var logs = new List<AuditLog>();
            while (reader.Read())
            {
                logs.Add(new AuditLog
                {
                    Id = Guid.Parse(reader["Id"].ToString()!),
                    UserName = reader["UserName"] == DBNull.Value ? null : reader["UserName"].ToString(),
                    Action = reader["Action"].ToString()!,
                    EntityType = reader["EntityType"] == DBNull.Value ? null : reader["EntityType"].ToString(),
                    EntityId = reader["EntityId"] == DBNull.Value ? null : reader["EntityId"].ToString(),
                    OldValue = reader["OldValue"] == DBNull.Value ? null : reader["OldValue"].ToString(),
                    NewValue = reader["NewValue"] == DBNull.Value ? null : reader["NewValue"].ToString(),
                    IpAddress = reader["IpAddress"] == DBNull.Value ? null : reader["IpAddress"].ToString(),
                    UserAgent = reader["UserAgent"] == DBNull.Value ? null : reader["UserAgent"].ToString(),
                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                });
            }

            return new PagedResult<AuditLog>(logs, totalCount, pageNumber, pageSize);
        }
    }
}
