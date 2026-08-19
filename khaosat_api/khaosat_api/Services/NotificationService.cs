using khaosat_api.DTOs;
using khaosat_api.Models;
using khaosat_api.Repositories.Interfaces;
using khaosat_api.Services.Interfaces;

namespace khaosat_api.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repository;
        private readonly IConfiguration _configuration;

        public NotificationService(
            INotificationRepository repository,
            IConfiguration configuration)
        {
            _repository = repository;
            _configuration = configuration;
        }

        public void Add(NotificationDto noti)
        {
            _repository.Add(noti);
        }

        public bool AddIfNotExists(NotificationDto noti, DateTime dayStartUtc, DateTime dayEndUtc)
        {
            return _repository.AddIfNotExists(noti, dayStartUtc, dayEndUtc);
        }

        public PagedResult<Notification> GetNotificationsByUserId(
            Guid userId,
            int pageNumber,
            int pageSize,
            int? typeFilter = null)
        {
            return _repository.GetNotificationsByUserId(
                userId,
                pageNumber,
                pageSize,
                typeFilter);
        }

        public void UpdateStatus(Guid id, Guid userId)
        {
            var updated = _repository.UpdateStatus(id, userId);
            if (!updated)
            {
                throw new KeyNotFoundException(
                    "Không tìm thấy thông báo hoặc thông báo không thuộc về người dùng.");
            }
        }
    }
}
