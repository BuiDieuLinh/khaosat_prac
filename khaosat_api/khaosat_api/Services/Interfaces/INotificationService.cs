using khaosat_api.DTOs;
using khaosat_api.Models;

namespace khaosat_api.Services.Interfaces
{
    public interface INotificationService
    {
        void Add(NotificationDto log);
        bool AddIfNotExists(NotificationDto log, DateTime dayStartUtc, DateTime dayEndUtc);
        void UpdateStatus(Guid id, Guid userId);
        PagedResult<Notification> GetNotificationsByUserId(Guid userId, int pageNumber, int pageSize, int? typeFilter = null);

    }
}
