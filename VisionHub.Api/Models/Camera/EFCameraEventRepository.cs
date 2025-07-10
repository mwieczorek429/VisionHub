
using Microsoft.EntityFrameworkCore;
using VisionHub.Api.Data;

namespace VisionHub.Api.Models.Camera
{
    public class EFCameraEventRepository : ICameraEventRepository
    {
        private readonly ApplicationDbContext _context;

        public EFCameraEventRepository(ApplicationDbContext context) 
        {
            _context = context;
        }

        public IQueryable<CameraEvent> CameraEvents => _context.CameraEvents;

        public async Task AddCameraEventAsync(CameraEvent cameraEvent)
        {
            await _context.CameraEvents.AddAsync(cameraEvent);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<CameraEvent>> GetCameraEventsAsync(int cameraId)
        {
            return await CameraEvents
                .Where(e => e.CameraId == cameraId)
                .ToListAsync();
        }

        public Task<IEnumerable<CameraEvent>> GetCameraEventsAsync(DateTimeOffset from, DateTimeOffset to)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<CameraEvent>> GetCameraEventsAsync(DateTimeOffset from, DateTimeOffset to, int cameraId)
        {
            throw new NotImplementedException();
        }

        public async Task<CameraEvent> GetLastCameraEventAsync(int cameraId)
        {
            return await CameraEvents
                .FirstOrDefaultAsync(e => e.CameraId == cameraId);
        }
    }
}
