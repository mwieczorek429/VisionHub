namespace VisionHub.Api.Models.Cameras
{
    public interface ICameraRepository
    {
        IQueryable<Camera> Cameras { get; }
        void AddCamera(Camera camera);
        Camera? GetCameraById(int? cameraId);
        void UpdateCamera(Camera camera);
        Task UpdateCameraAsync(Camera camera);
        void DeleteCamera(int cameraId);
        bool CanConnect();
    }
}
