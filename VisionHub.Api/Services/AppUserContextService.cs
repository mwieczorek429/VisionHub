using System.Security.Claims;

namespace VisionHub.Api.Services
{
    public class AppUserContextService : IAppUserContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AppUserContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int AppUserId => int.Parse(
            _httpContextAccessor.HttpContext?.User?
                .FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User is not authenticated."));

        public bool TryGetUserId(out int userId)
        {
            userId = 0;
            var userIdStr = _httpContextAccessor.HttpContext?.User?
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return int.TryParse(userIdStr, out userId);
        }

        public bool IsAuthenticated =>
            _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
    }
}

