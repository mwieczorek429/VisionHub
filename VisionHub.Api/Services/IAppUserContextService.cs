namespace VisionHub.Api.Services
{
    public interface IAppUserContextService
    {
        int AppUserId { get; }
        bool TryGetUserId(out int userId);
        bool IsAuthenticated { get; }
    }
}
