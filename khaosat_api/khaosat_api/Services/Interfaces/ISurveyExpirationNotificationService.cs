namespace khaosat_api.Services.Interfaces
{
    public interface ISurveyExpirationNotificationService
    {
        Task CreateRemindersAsync(CancellationToken cancellationToken = default);
    }
}
