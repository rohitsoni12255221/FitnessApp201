using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserAPI.AzureServices;
using UserAPI.DAL;
using UserAPI.Models;
using UserAPI.Models.DTOs;
using UserAPI.Services;
using WorkoutService.Models.DTO;

namespace UserAPI.AuthRepo
{
    public class UserRepository : IUserRepository
    {
        private readonly UserDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IBlobService _blobService;
        private readonly IQueueService _queueService;
        private readonly EmailService _emailService;

        public UserRepository(UserDbContext context, IConfiguration configuration, IBlobService blobService, IQueueService queueService, EmailService emailService  )
        {
            _context = context;
            _configuration = configuration;
            _blobService = blobService;
            _queueService = queueService;
            _emailService = emailService;
        }

        public async Task<User> CreateAsync(RegisterDto dto)
        {
            if (dto.Role == "Admin")
                throw new Exception("You are not authorized to register as Admin.");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            try
            {
                var users = await _context.Users
                    .FromSqlInterpolated($@"
                EXEC sp_RegisterUser 
                    @UserName = {dto.UserName}, 
                    @PasswordHash = {passwordHash}, 
                    @Email = {dto.Email}, 
                    @FitnessGoal = {dto.FitnessGoal ?? (object)DBNull.Value}, 
                    @Role = {"User"}")
                    .AsNoTracking()
                    .ToListAsync(); // Execute and get results in-memory

                var user = users.FirstOrDefault(); // Now safe to use LINQ

                if (user == null)
                    throw new Exception("User could not be created.");

                return user;
            }
            catch (SqlException ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task<List<User>> GetAllUsersAsync(ClaimsPrincipal userPrincipal)
        {
            var role = userPrincipal.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "Admin")
                throw new UnauthorizedAccessException("Only admins can view all users.");

            var users = await _context.Users
                .FromSqlRaw("EXEC sp_GetAllUsers")
                .AsNoTracking()
                .ToListAsync();

            return users;
        }

       
        public async Task<User> UpdateDetailAsync(int id, UpdateUserDTO updateUser, ClaimsPrincipal userPrincipal)
        {
            if (updateUser == null)
                throw new ArgumentNullException(nameof(updateUser));

            var userIdClaim = userPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = userPrincipal.FindFirst(ClaimTypes.Role)?.Value;

            if (userIdClaim == null || role == null)
                throw new Exception("Unauthorized");

            int loggedInUserId = int.Parse(userIdClaim);

            // Both Admin and User can update ONLY their own details
            if (loggedInUserId != id)
                throw new Exception("You are not allowed to update other users.");

            string? passwordHash = null;

            if (!string.IsNullOrWhiteSpace(updateUser.Password))
            {
                passwordHash = BCrypt.Net.BCrypt.HashPassword(updateUser.Password);
            }

            try
            {
                var users = await _context.Users
                    .FromSqlInterpolated($@"
                EXEC sp_UpdateUserDetail
                    @Id = {id},
                    @UserName = {updateUser.UserName},
                    @Email = {updateUser.Email},
                    @FitnessGoal = {updateUser.FitnessGoal ?? (object)DBNull.Value},
                    @PasswordHash = {passwordHash ?? (object)DBNull.Value},
                    @Role = {updateUser.Role ?? (object)DBNull.Value},
                    @IsAdmin = {(role == "Admin" ? 1 : 0)}")
                    .AsNoTracking()
                    .ToListAsync();

                var updatedUser = users.FirstOrDefault();
                if (updatedUser == null)
                    throw new Exception("Update failed or user not found.");

                return updatedUser;
            }
            catch (SqlException ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<User> GetUserByIdAsync(int id, ClaimsPrincipal userPrincipal)
        {
            var userIdClaim = userPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = userPrincipal.FindFirst(ClaimTypes.Role)?.Value;

            if (userIdClaim == null || role == null)
                throw new UnauthorizedAccessException("Invalid token.");

            var callerId = int.Parse(userIdClaim);

            if (role == "User" && callerId != id)
                throw new UnauthorizedAccessException("You are not allowed to view other users' details.");

            var users = await _context.Users
                .FromSqlInterpolated($"EXEC sp_GetUserById @UserId = {id}")
                .AsNoTracking()
                .ToListAsync();

            var user = users.FirstOrDefault();

            if (user == null)
                throw new KeyNotFoundException("User not found.");

            return user;
        }


        public async Task<bool> DeleteUserAsync(int id, ClaimsPrincipal userPrincipal)
        {
            var role = userPrincipal.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "Admin")
                throw new UnauthorizedAccessException("Only admins can delete users.");

            try
            {
                var result = await _context.Users
                    .FromSqlInterpolated($"EXEC sp_DeleteUserByIdSafe @UserId = {id}")
                    .AsNoTracking()
                    .ToListAsync();

                // If nothing was returned, user didn't exist
                if (!result.Any())
                    throw new Exception("User not found.");

                return true;
            }
            catch (SqlException ex)
            {
                throw new Exception($"SQL Error: {ex.Message}");
            }
        }

        public async Task<bool> VerifyEmailAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            return user != null;
        }

        //public async Task<string> SendOtpAsync(string email)
        //{
        //    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        //    if (user == null)
        //        throw new Exception("Email not found.");

        //    var otp = new Random().Next(100000, 999999).ToString();
        //    user.OtpCode = otp;
        //    user.OtpExpiry = DateTime.UtcNow.AddMinutes(5); // valid for 5 minutes

        //    await _context.SaveChangesAsync();

        //    return otp; // later you will send this via email
        //}
        public async Task<string> SendOtpAsync(string email)
        {
            // Check if user exists
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                throw new Exception("Email not found.");
            }

            // Generate OTP
            var otp = new Random().Next(100000, 999999).ToString();

            // Save OTP to database and set expiration
            user.OtpCode = otp;
            user.OtpExpiry = DateTime.UtcNow.AddMinutes(5); // OTP valid for 5 minutes
            await _context.SaveChangesAsync();

            // Send OTP via email using EmailService
            await _emailService.SendOtpAsync(email, otp);

            return otp; // Optionally, return OTP for testing (do not return OTP in production)
        }
        
        public async Task<bool> ResetPasswordAsync(string email, string otp, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                throw new Exception("Invalid email.");

            if (user.OtpCode != otp || user.OtpExpiry < DateTime.UtcNow)
                throw new Exception("Invalid or expired OTP.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.OtpCode = null;
            user.OtpExpiry = null;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<string> UpdateProfilePictureAsync(IFormFile file, ClaimsPrincipal userPrincipal)
        {
            var userIdClaim = userPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("Invalid token.");

            int userId = int.Parse(userIdClaim);
            string fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            // Upload to Azure Blob Storage
            string imageUrl = await _blobService.UploadAsync(file, fileName);

            try
            {
                // ✅ Ensure the parameter name matches the stored procedure exactly
                var result = await _context.Database.ExecuteSqlInterpolatedAsync($@"
            EXEC sp_UpdateProfileImage 
                @UserId = {userId}, 
                @ProfileImageUrl = {imageUrl}");

                if (result == 0)
                    throw new Exception("Profile image update failed.");

                return imageUrl;
            }
            catch (SqlException ex)
            {
                throw new Exception($"SQL Error: {ex.Message}");
            }
        }

        public async Task<ResponseDto> ToggleUserStatusAsync(int loggedInUserId, int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return new ResponseDto
                    {
                        Status = false,
                        Message = "User not found"
                    };
                }

                // Toggle the status
                user.IsActive = !user.IsActive;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                var payload = new
                {
                    UserId = userId,
                    PerformedBy = loggedInUserId
                };
                await _queueService.SendMessageAsync(actionType: user.IsActive ? "UserActivated" : "UserDeactivated",
                    payload, source: "AdminDashboard");


                return new ResponseDto
                {
                    Status = true,
                    Message = user.IsActive ? "User activated successfully" : "User deactivated  successfully"
                };
            }
            catch (Exception ex)
            {


                return new ResponseDto
                {
                    Status = false,
                    Message = ex.Message
                };
            }
        }





    }
}
