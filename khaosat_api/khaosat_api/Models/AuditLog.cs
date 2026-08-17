using System;

namespace khaosat_api.Models
{
    public class AuditLog
    {
        public Guid Id { get; set; }
        public string? UserName { get; set; }
        public string Action { get; set; } = string.Empty; // e.g., 'CREATE_SURVEY', 'UPDATE_SURVEY', 'CHANGE_ACCESS_TYPE', 'CHANGE_ANONYMOUS_MODE', 'CLOSE_SURVEY'
        public string? EntityType { get; set; }
        public string? EntityId { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
