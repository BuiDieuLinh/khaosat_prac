using khaosat_api.DTOs;
using khaosat_api.Models;
using System.Collections.Generic;

namespace khaosat_api.Repositories.Interfaces
{
    public interface IAuditLogRepository
    {
        void Add(AuditLog log);
        PagedResult<AuditLog> GetLogs(int pageNumber, int pageSize, string? actionFilter = null, string? searchKeyword = null);
    }
}
