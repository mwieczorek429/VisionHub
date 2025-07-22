namespace VisionHub.Api.Models.Cameras
{
    public interface ICameraEventRepository
    {
        IQueryable<CameraEvent> CameraEvents { get; }
        Task<IEnumerable<CameraEventWithCameraIdDto>> GetCameraEventsAsync();
        Task<IEnumerable<CameraEvent>> GetCameraEventsAsync(DateTimeOffset from, DateTimeOffset to);
        Task<IEnumerable<CameraEvent>> GetCameraEventsAsync(DateTimeOffset from, DateTimeOffset to, int cameraId);
        Task<CameraEventWithCameraIdDto> GetLastCameraEventAsync();
        Task AddCameraEventAsync(CameraEvent cameraEvent);

    }
}
