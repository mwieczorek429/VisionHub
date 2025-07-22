namespace VisionHub.Api.Models.Cameras
{
    public interface ICameraRepository
    {
        IQueryable<Camera> Cameras { get; }
        Task<List<CameraSummaryDto>> GetAllSummariesAsync();
        void AddCamera(Camera camera);
        Camera? GetCameraById(int? cameraId);
        void UpdateCamera(Camera camera);
        Task UpdateCameraFromBackgroundAsync(Camera camera);
        void DeleteCamera(int cameraId);
        bool CanConnect();
        Task<CameraDetailsDto> GetCameraDetailsAsync(int cameraId);
    }
}
