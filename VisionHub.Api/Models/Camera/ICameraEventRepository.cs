namespace VisionHub.Api.Models.Camera
{
    public interface ICameraEventRepository
    {
        IQueryable<CameraEvent> CameraEvents { get; }
        Task<IEnumerable<CameraEvent>> GetCameraEventsAsync(int cameraId);
        Task<IEnumerable<CameraEvent>> GetCameraEventsAsync(DateTimeOffset from, DateTimeOffset to);
        Task<IEnumerable<CameraEvent>> GetCameraEventsAsync(DateTimeOffset from, DateTimeOffset to, int cameraId);
        Task<CameraEvent>GetLastCameraEventAsync(int cameraId);
        Task AddCameraEventAsync(CameraEvent cameraEvent);

    }
}
