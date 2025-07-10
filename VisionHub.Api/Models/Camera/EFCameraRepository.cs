
using VisionHub.Api.Data;

namespace VisionHub.Api.Models.Camera
{
    public class EFCameraRepository : ICameraRepository
    {
        private readonly ApplicationDbContext _context;
        public EFCameraRepository(ApplicationDbContext context) 
        {
            _context = context;
        }
        public IQueryable<Camera> Cameras => _context.Cameras;

        public void AddCamera(Camera camera)
        {
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
            if (camera != null)
            {
                _context.Cameras.Remove(camera);
            }
        }

        public Camera? GetCameraById(int? cameraId)
        {
            return Cameras.FirstOrDefault(c => c.Id == cameraId);
        }

        public void UpdateCamera(Camera camera)
        {
            _context.Update(camera);
            _context.SaveChanges();
        }

        public Task UpdateCameraAsync(Camera camera)
        {
            _context.Update(camera);
            return _context.SaveChangesAsync();
        }
    }
}
