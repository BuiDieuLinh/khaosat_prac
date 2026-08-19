using khaosat_api.DTOs;
using khaosat_api.Models;

namespace khaosat_api.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        void Add(NotificationDto log);
        bool AddIfNotExists(NotificationDto log, DateTime dayStartUtc, DateTime dayEndUtc);
        bool UpdateStatus(Guid í, Guid userId);
        PagedResult<Notification> GetNotificationsByUserId(Guid userId, int pageNumber, int pageSize, int? typeFilter = null);
    }
}
