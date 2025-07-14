using VisionHub.Api.Models.Auth;

namespace VisionHub.Api.Models.Cameras
{
    public interface IAppUserRepository
    {
        Task<AppUser?> GetByIdAsync(int id);
        Task<AppUser?> GetByLoginAsync(string username);
        Task<IEnumerable<AppUser>> GetAllAsync();
        Task AddAsync(AppUser user);
        Task UpdateAsync(AppUser user);
        Task DeleteAsync(int id);
        Task<bool> UserExistsAsync(string username);
    }
}
