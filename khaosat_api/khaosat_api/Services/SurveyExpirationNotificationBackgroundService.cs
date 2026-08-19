using khaosat_api.Services.Interfaces;

namespace khaosat_api.Services
{
    public class SurveyExpirationNotificationBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SurveyExpirationNotificationBackgroundService> _logger;

        public SurveyExpirationNotificationBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<SurveyExpirationNotificationBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var nextRunUtc = GetNextRunUtc();
                _logger.LogInformation("Next survey expiration reminder job is scheduled for {NextRunUtc:O}.", nextRunUtc);

                try
                {
                    await Task.Delay(nextRunUtc - DateTime.UtcNow, stoppingToken);

                    using var scope = _scopeFactory.CreateScope();
                    var notificationService = scope.ServiceProvider
                        .GetRequiredService<ISurveyExpirationNotificationService>();
                    await notificationService.CreateRemindersAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Survey expiration reminder job failed.");
                }
            }
        }

        private static DateTime GetNextRunUtc()
        {
            var timeZone = GetBusinessTimeZone();
            var nowUtc = DateTime.UtcNow;
            var nowInBusinessTimeZone = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, timeZone);
            //var runAt = nowInBusinessTimeZone.Date.AddHours(8).AddMinutes(30);
            var runAt = nowInBusinessTimeZone.AddMinutes(2);

            // If the API starts after 08:30, it waits for tomorrow instead of catching up.
            if (nowInBusinessTimeZone >= runAt)
            {
                runAt = runAt.AddDays(1);
            }

            return TimeZoneInfo.ConvertTimeToUtc(
                DateTime.SpecifyKind(runAt, DateTimeKind.Unspecified),
                timeZone);
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
