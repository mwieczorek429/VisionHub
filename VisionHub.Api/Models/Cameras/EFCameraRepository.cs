using Microsoft.EntityFrameworkCore;
using VisionHub.Api.Data;
using VisionHub.Api.Services;

namespace VisionHub.Api.Models.Cameras
{
    public class EFCameraRepository : ICameraRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IAppUserContextService _userContext;
        public EFCameraRepository(ApplicationDbContext context, IAppUserContextService userContext) 
        {
            _context = context;
            _userContext = userContext;

        }
        public IQueryable<Camera> Cameras => _context.Cameras;

        public void AddCamera(Camera camera)
        {
            camera.AppUserId = _userContext.AppUserId;
            _context.Add(camera);
            _context.SaveChanges();
        }

        public bool CanConnect()
        {
            try
            {
                _context.Database.CanConnect();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public void DeleteCamera(int cameraId)
        {
            var camera = GetCameraById(cameraId);
            if (camera != null && camera.AppUserId == _userContext.AppUserId)
            {
                _context.Cameras.Remove(camera);
                _context.SaveChanges();
            }
        }

        public Camera? GetCameraById(int? cameraId)
        {
            return _context.Cameras
                .FirstOrDefault(c => c.Id == cameraId && c.AppUserId == _userContext.AppUserId);
        }

        public void UpdateCamera(Camera camera)
        {
            if (camera.AppUserId != _userContext.AppUserId)
                throw new UnauthorizedAccessException("You do not have permission to update this camera.");

            _context.Update(camera);
            _context.SaveChanges();
        }

        public Task UpdateCameraFromBackgroundAsync(Camera camera)
        {
            _context.Update(camera);
            return _context.SaveChangesAsync();
        }

        public async Task<List<CameraSummaryDto>> GetAllSummariesAsync()
        {
            return await _context.Cameras
                .Where(c => c.AppUserId == _userContext.AppUserId)
                .Select(camera => new CameraSummaryDto
                {
                    Id = camera.Id,
                    Name = camera.Name,
                    Url = camera.Url,
                    LastEventTimestamp = camera.CameraEvents
                        .OrderByDescending(e => e.Timestamp)
                        .Select(e => (DateTimeOffset)e.Timestamp)
                        .FirstOrDefault(),
                    LastEventDetectedObject = camera.CameraEvents
                        .OrderByDescending(e => e.Timestamp)
                        .Select(e => e.Object)
                        .FirstOrDefault(),
                    LastEventMotionDetected = camera.CameraEvents
                        .OrderByDescending(e => e.Timestamp)
                        .Select(e => (bool?)e.MotionDetected)
                        .FirstOrDefault()
                })
                .ToListAsync();
        }

        public async Task<CameraDetailsDto> GetCameraDetailsAsync(int cameraId)
        {
            var camera = await _context.Cameras
                .Include(c => c.CameraEvents)
                .FirstOrDefaultAsync(c => c.Id == cameraId && c.AppUserId == _userContext.AppUserId);

            if (camera == null)
                return null;

            return new CameraDetailsDto
            {
                Id = camera.Id,
                Name = camera.Name,
                Url = camera.Url,
                CameraEvents = camera.CameraEvents
                    .OrderByDescending(e => e.Timestamp)
                    .Select(e => new CameraEventDto
                    {
                        Id = e.Id,
                        MotionDetected = e.MotionDetected,
                        Timestamp = e.Timestamp,
                        Object = e.Object
                    })
                    .ToList()
            };
        }
    }
}
