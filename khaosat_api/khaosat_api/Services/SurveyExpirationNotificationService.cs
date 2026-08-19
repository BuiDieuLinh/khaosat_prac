using khaosat_api.DTOs;
using khaosat_api.Hubs;
using khaosat_api.Repositories.Interfaces;
using khaosat_api.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace khaosat_api.Services
{
    public class SurveyExpirationNotificationService : ISurveyExpirationNotificationService
    {
        private const int ReminderDaysBeforeExpiration = 3;
        private const int SurveyExpirationNotificationType = 1;

        private readonly ISurveyRepository _surveyRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<NotificationHub> _notificationHub;
        private readonly ILogger<SurveyExpirationNotificationService> _logger;
        private readonly TimeZoneInfo _businessTimeZone;

        public SurveyExpirationNotificationService(
            ISurveyRepository surveyRepository,
            IEmployeeRepository employeeRepository,
            INotificationService notificationService,
            IHubContext<NotificationHub> notificationHub,
            ILogger<SurveyExpirationNotificationService> logger)
        {
            _surveyRepository = surveyRepository;
            _employeeRepository = employeeRepository;
            _notificationService = notificationService;
            _notificationHub = notificationHub;
            _logger = logger;
            _businessTimeZone = GetBusinessTimeZone();
        }

        public async Task CreateRemindersAsync(CancellationToken cancellationToken = default)
        {
            var nowUtc = DateTime.UtcNow;
            var nowInBusinessTimeZone = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, _businessTimeZone);
            var reminderDate = nowInBusinessTimeZone.Date.AddDays(ReminderDaysBeforeExpiration);
            var startUtc = ToUtc(reminderDate);
            var endUtc = ToUtc(reminderDate.AddDays(1));
            var notificationDayStartUtc = ToUtc(nowInBusinessTimeZone.Date);
            var notificationDayEndUtc = ToUtc(nowInBusinessTimeZone.Date.AddDays(1));

            var surveys = _surveyRepository.GetSurveysExpiringOn(startUtc, endUtc);
            var administratorIds = _employeeRepository.GetActiveAdministratorIds();

            foreach (var survey in surveys)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!survey.EndDate.HasValue)
                {
                    continue;
                }
                
                var endDateInBusinessTimeZone = TimeZoneInfo.ConvertTimeFromUtc(
                    DateTime.SpecifyKind(survey.EndDate.Value, DateTimeKind.Utc),
                    _businessTimeZone);
                foreach (var administratorId in administratorIds)
                {
                    var notification = new NotificationDto
                    {
                        Id = Guid.NewGuid(),
                        UserId = administratorId,
                        Title = "Khảo sát sắp hết hạn",
                        Message = $"Khảo sát '{survey.Name}' sẽ hết hạn lúc {endDateInBusinessTimeZone:HH:mm dd/MM/yyyy}.",
                        Link = $"/Survey/Detail/{survey.Id}",
                        Type = SurveyExpirationNotificationType,
                        IsRead = false,
                        CreatedDate = nowUtc
                    };

                    if (!_notificationService.AddIfNotExists(notification, notificationDayStartUtc, notificationDayEndUtc))
                    {
                        continue;
                    }

                    await _notificationHub.Clients.User(administratorId.ToString())
                        .SendAsync("ReceiveNotification", notification, cancellationToken);
                }
            }

            _logger.LogInformation(
                "Survey expiration reminder scan completed. {SurveyCount} survey(s), {AdministratorCount} administrator(s), reminder date {ReminderDate:yyyy-MM-dd}.",
                surveys.Count,
                administratorIds.Count,
                reminderDate);
        }

        private DateTime ToUtc(DateTime localDateTime)
        {
            return TimeZoneInfo.ConvertTimeToUtc(
                DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified),
                _businessTimeZone);
        }

        private static TimeZoneInfo GetBusinessTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Bangkok");
            }
        }
    }
}
