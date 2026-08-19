using khaosat_api.Data;
using khaosat_api.DTOs;
using khaosat_api.Models;
using khaosat_api.Repositories.Interfaces;
using System.Data;
using System.Data.SqlClient;

namespace khaosat_api.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly SqlConnectionFactory _factory;

        public NotificationRepository(SqlConnectionFactory factory)
        {
            _factory = factory;
        }

        public void Add(NotificationDto log)
        {
            using var conn = _factory.Create();
            conn.Open();

            const string sql = @"
                INSERT INTO Notification (Id, UserId, Title, Message, Link, Type, IsRead, CreatedDate)
                VALUES (@Id, @UserId, @Title, @Message, @Link, @Type, @IsRead, @CreatedDate)";

            using var cmd = new SqlCommand(sql, conn);
            AddNotificationParameters(cmd, log);
            cmd.ExecuteNonQuery();
        }

        public bool AddIfNotExists(NotificationDto log, DateTime dayStartUtc, DateTime dayEndUtc)
        {
            using var conn = _factory.Create();
            conn.Open();

            const string sql = @"
                IF NOT EXISTS (
                    SELECT 1
                    FROM Notification WITH (UPDLOCK, HOLDLOCK)
                    WHERE UserId = @UserId
                      AND Type = @Type
                      AND Link = @Link
                      AND CreatedDate >= @DayStartUtc
                      AND CreatedDate < @DayEndUtc
                )
                BEGIN
                    INSERT INTO Notification (Id, UserId, Title, Message, Link, Type, IsRead, CreatedDate)
                    VALUES (@Id, @UserId, @Title, @Message, @Link, @Type, @IsRead, @CreatedDate);
                    SELECT CAST(1 AS bit);
                END
                ELSE
                BEGIN
                    SELECT CAST(0 AS bit);
                END";

            using var cmd = new SqlCommand(sql, conn);
            AddNotificationParameters(cmd, log);
            cmd.Parameters.AddWithValue("@DayStartUtc", DateTime.SpecifyKind(dayStartUtc, DateTimeKind.Unspecified));
            cmd.Parameters.AddWithValue("@DayEndUtc", DateTime.SpecifyKind(dayEndUtc, DateTimeKind.Unspecified));
            return Convert.ToBoolean(cmd.ExecuteScalar());
        }

        public PagedResult<Notification> GetNotificationsByUserId(
            Guid userId,
            int pageNumber,
            int pageSize,
            int? typeFilter = null)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, 100);
            var skip = (pageNumber - 1) * pageSize;

            using var conn = _factory.Create();
            conn.Open();

            var whereSql = "WHERE UserId = @UserId" + (typeFilter.HasValue ? " AND Type = @TypeFilter" : string.Empty);
            using var countCmd = new SqlCommand($"SELECT COUNT(1) FROM Notification {whereSql}", conn);
            AddUserFilterParameters(countCmd, userId, typeFilter);
            var totalCount = Convert.ToInt32(countCmd.ExecuteScalar());

            if (totalCount == 0)
            {
                return new PagedResult<Notification>(new List<Notification>(), 0, pageNumber, pageSize);
            }

            var querySql = $@"
                SELECT Id, UserId, Title, Message, Link, Type, IsRead, CreatedDate
                FROM Notification
                {whereSql}
                ORDER BY CreatedDate DESC
                OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY";
            using var queryCmd = new SqlCommand(querySql, conn);
            AddUserFilterParameters(queryCmd, userId, typeFilter);
            queryCmd.Parameters.Add("@Skip", SqlDbType.Int).Value = skip;
            queryCmd.Parameters.Add("@Take", SqlDbType.Int).Value = pageSize;

            using var reader = queryCmd.ExecuteReader();
            var notifications = new List<Notification>();
            while (reader.Read())
            {
                notifications.Add(new Notification
                {
                    Id = reader.GetGuid(reader.GetOrdinal("Id")),
                    UserId = reader.GetGuid(reader.GetOrdinal("UserId")),
                    Title = reader.GetString(reader.GetOrdinal("Title")),
                    Message = reader.GetString(reader.GetOrdinal("Message")),
                    Link = reader.IsDBNull(reader.GetOrdinal("Link")) ? null : reader.GetString(reader.GetOrdinal("Link")),
                    Type = reader.GetInt32(reader.GetOrdinal("Type")),
                    IsRead = reader.GetBoolean(reader.GetOrdinal("IsRead")),
                    CreatedDate = DateTime.SpecifyKind(reader.GetDateTime(reader.GetOrdinal("CreatedDate")), DateTimeKind.Utc)
                });
            }

            return new PagedResult<Notification>(notifications, totalCount, pageNumber, pageSize);
        }

        public bool UpdateStatus(Guid id, Guid userId)
        {
            using var conn = _factory.Create();
            conn.Open();

            const string sql = @"
                UPDATE Notification SET IsRead = 1
                WHERE Id = @Id AND UserId = @UserId AND IsRead = 0";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = id;
            cmd.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;
            return cmd.ExecuteNonQuery() > 0;
        }

        private static void AddNotificationParameters(SqlCommand cmd, NotificationDto log)
        {
            cmd.Parameters.AddWithValue("@Id", log.Id == Guid.Empty ? Guid.NewGuid() : log.Id);
            cmd.Parameters.AddWithValue("@UserId", log.UserId);
            cmd.Parameters.AddWithValue("@Title", log.Title);
            cmd.Parameters.AddWithValue("@Message", log.Message);
            cmd.Parameters.AddWithValue("@Link", (object?)log.Link ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Type", log.Type);
            cmd.Parameters.AddWithValue("@IsRead", log.IsRead);
            cmd.Parameters.AddWithValue("@CreatedDate", log.CreatedDate);
        }

        private static void AddUserFilterParameters(SqlCommand cmd, Guid userId, int? typeFilter)
        {
            cmd.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;
            if (typeFilter.HasValue)
            {
                cmd.Parameters.Add("@TypeFilter", SqlDbType.Int).Value = typeFilter.Value;
            }
        }
    }
}
