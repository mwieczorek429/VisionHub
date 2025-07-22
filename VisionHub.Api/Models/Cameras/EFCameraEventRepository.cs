
using Microsoft.EntityFrameworkCore;
using VisionHub.Api.Data;
using VisionHub.Api.Services;

namespace VisionHub.Api.Models.Cameras
{
    public class EFCameraEventRepository : ICameraEventRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IAppUserContextService? _userContext;

        public EFCameraEventRepository(ApplicationDbContext context, IAppUserContextService userContext) 
        {
            _context = context;
            _userContext = userContext;
        }

        public IQueryable<CameraEvent> CameraEvents => _context.CameraEvents;

        public async Task AddCameraEventAsync(CameraEvent cameraEvent)
        {
            await _context.CameraEvents.AddAsync(cameraEvent);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<CameraEventWithCameraIdDto>> GetCameraEventsAsync()
        {
            var userCameraIds = await _context.Cameras
                .Where(c => c.AppUserId == _userContext.AppUserId)
                .Select(c => c.Id)
                .ToListAsync();

            if (!userCameraIds.Any())
                return null;

            var events = await _context.CameraEvents
                .Where(e => userCameraIds.Contains(e.CameraId))
                .OrderByDescending(e => e.Timestamp)
                .Select(e => new CameraEventWithCameraIdDto
                {
                    Id = e.Id,
                    CameraId = e.CameraId,
                    MotionDetected = e.MotionDetected,
                    Timestamp = e.Timestamp,
                    Object = e.Object,
                })
                .ToListAsync();

            return events;
        }

        public Task<IEnumerable<CameraEvent>> GetCameraEventsAsync(DateTimeOffset from, DateTimeOffset to)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<CameraEvent>> GetCameraEventsAsync(DateTimeOffset from, DateTimeOffset to, int cameraId)
        {
            throw new NotImplementedException();
        }

        public async Task<CameraEventWithCameraIdDto> GetLastCameraEventAsync()
        {
            var userCameraIds = await _context.Cameras
                .Where(c => c.AppUserId == _userContext.AppUserId)
                .Select(c => c.Id)
                .ToListAsync();

            if (!userCameraIds.Any())
                return null;

            var lastEvent = await _context.CameraEvents
                .Where(e => userCameraIds.Contains(e.CameraId))
                .OrderByDescending(e => e.Timestamp)
                .Select(e => new CameraEventWithCameraIdDto
                {
                    Id = e.Id,
                    CameraId = e.CameraId,
                    MotionDetected = e.MotionDetected,
                    Timestamp = e.Timestamp,
                    Object = e.Object,
                })
                .FirstOrDefaultAsync();

            return lastEvent;
        }
    }
}
