using System.Security.Claims;
using UserAPI.Models;
using UserAPI.Models.DTOs;
using WorkoutService.Models.DTO;

namespace UserAPI
{
    public interface IUserRepository
    {
        Task<User> CreateAsync(RegisterDto dto);
        Task<List<User>> GetAllUsersAsync(ClaimsPrincipal userPrincipal);
        Task<User> GetUserByIdAsync(int id, ClaimsPrincipal userPrincipal);
        Task<User> UpdateDetailAsync(int id, UpdateUserDTO updateUser, ClaimsPrincipal userPrincipal);
        Task<bool> DeleteUserAsync(int id, ClaimsPrincipal userPrincipal);
        Task<bool> VerifyEmailAsync(string email);
        Task<string> SendOtpAsync(string email);
        Task<bool> ResetPasswordAsync(string email, string otp, string newPassword);
        Task<string> UpdateProfilePictureAsync(IFormFile file, ClaimsPrincipal user);
        Task<ResponseDto> ToggleUserStatusAsync(int loggedInUserId, int userId);



    }
}
