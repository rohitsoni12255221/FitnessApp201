using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserAPI.AuthRepo;
using UserAPI.AzureServices;
using UserAPI.DAL;
using UserAPI.Models.DTOs;

namespace UserAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IBlobService _blobService;
        private readonly ILogger<UserController> _logger;


        public UserController(IUserRepository userAuth, IBlobService blobService, UserDbContext userDbContext, ILogger<UserController> logger)
        {
            _userRepository = userAuth;
            _blobService = blobService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto request)
        {
            try
            {
                var user = await _userRepository.CreateAsync(request);
                _logger.LogInformation("User registered successfully: {Email}", user.Email);
                return Ok(new { message = "Registration successful", user.Id, user.UserName, user.Email, user.FitnessGoal });
            }
           
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error during user registration: {Email}", request.Email);
                return StatusCode(500, new { error = "Something went wrong." });
            }

        }

        [Authorize(Policy = "Admin")]
        [HttpGet("all-users")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userRepository.GetAllUsersAsync(User);
                _logger.LogInformation("Admin user '{User}' fetched all users.", User.Identity?.Name);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching all users by admin '{User}'", User.Identity?.Name);
                return StatusCode(500, new { error = "An error occurred while fetching users." });
            }
        }


        [Authorize]  // Both Admin and User
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(id, User);

                _logger.LogInformation("User {UserId} retrieved by {Requester}.", id, User.Identity?.Name);

                return Ok(new

                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.FitnessGoal,
                    user.Role,
                    user.CreatedAt,
                    ProfileUrl = user.ProfileUrl
                });
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogWarning("Unauthorized access attempt by {Requester} for user ID {UserId}.", User.Identity?.Name, id);
                return Forbid(); // 403
            }
            catch (KeyNotFoundException knf)
            {
                _logger.LogWarning("User ID {UserId} not found. Requested by {Requester}.", id, User.Identity?.Name);
                return NotFound(new { error = knf.Message }); // 404
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving user ID {UserId} by {Requester}.", id, User.Identity?.Name);
                return StatusCode(500, new { error = "An unexpected error occurred." }); // 500
            }
        }




        [Authorize(Roles = "Admin,User")]
        [HttpPut("update-user/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDTO updateUser)
        {
            if (updateUser == null)
            {
                _logger.LogWarning("Update request is null for user ID {UserId}", id);
                return BadRequest(new { error = "Invalid user update request." });
            }

            try
            {
                var updatedUser = await _userRepository.UpdateDetailAsync(id, updateUser, User);
                _logger.LogInformation("User ID {UserId} updated successfully by {Updater}", id, User.Identity?.Name);

                return Ok(new
                {
                    message = "User updated successfully",
                    user = new
                    {
                        updatedUser.Id,
                        updatedUser.UserName,
                        updatedUser.Email,
                        updatedUser.FitnessGoal,
                        updatedUser.Role
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user ID {UserId} by {Updater}", id, User.Identity?.Name);
                return StatusCode(500, new { error = "An unexpected error occurred." });
            }
        }
    

        [Authorize(Policy = "Admin")]
        [HttpDelete("delete-user/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var result = await _userRepository.DeleteUserAsync(id, User);
                if (result)
                {
                    _logger.LogInformation("User ID {UserId} deleted by {Admin}", id, User.Identity?.Name);
                    return Ok(new { message = "User deleted successfully." });
                }

                _logger.LogWarning("Failed to delete user ID {UserId} by {Admin}", id, User.Identity?.Name);
                return BadRequest("Failed to delete user.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user ID {UserId} by {Admin}", id, User.Identity?.Name);
                return StatusCode(500, new { error = "An unexpected error occurred." });
            }
        }


        [AllowAnonymous]
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto dto)
        {
            _logger.LogInformation("Verifying email: {Email}", dto.Email);

            var emailExists = await _userRepository.VerifyEmailAsync(dto.Email);

            if (!emailExists)
            {
                _logger.LogWarning("Email verification failed. Email not found: {Email}", dto.Email);
                return BadRequest(new { Error = "Email not found." });
            }

            _logger.LogInformation("Email verified successfully: {Email}", dto.Email);
            return Ok(new { Message = "Email verified successfully." });
        }

        [AllowAnonymous]
        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] SendOtpDto dto)
        {
            try
            {
                _logger.LogInformation("Sending OTP to email: {Email}", dto.Email);
                var otp = await _userRepository.SendOtpAsync(dto.Email);
                _logger.LogInformation("OTP sent successfully to email: {Email}", dto.Email);
                return Ok(new { message = "OTP sent successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending OTP to email: {Email}", dto.Email);
                return BadRequest(new { Error = ex.Message });
            }
        }


        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            _logger.LogInformation("Attempting password reset for email: {Email}", dto.Email);

            var resetSuccessful = await _userRepository.ResetPasswordAsync(dto.Email, dto.Otp, dto.NewPassword);

            if (!resetSuccessful)
            {
                _logger.LogWarning("Password reset failed for email: {Email}", dto.Email);
                return BadRequest(new { Message = "Password reset failed." });
            }

            _logger.LogInformation("Password reset successful for email: {Email}", dto.Email);
            return Ok(new { Message = "Password reset successfully." });
        }


        [Authorize]
        [HttpPost("update-profile-picture")]
        public async Task<IActionResult> UpdateProfilePicture([FromForm] ProfilePictureDto dto)
        {
            try
            {
                _logger.LogInformation("User {UserId} is uploading a profile picture.",
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                var result = await _userRepository.UpdateProfilePictureAsync(dto.File, User);

                _logger.LogInformation("Profile picture uploaded successfully for user {UserId}.",
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                return Ok(new { message = "Image uploaded successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading profile picture for user {UserId}.",
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPut("toggle-status/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleUserStatus(int userId)
        {
            var loggedInUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (loggedInUserIdClaim == null)
            {
                _logger.LogWarning("Unauthorized access attempt while toggling user status.");
                return Unauthorized(new ResponseDto
                {
                    Status = false,
                    Message = "Unauthorized access"
                });
            }

            int loggedInUserId = int.Parse(loggedInUserIdClaim.Value);

            _logger.LogInformation("Admin user {AdminId} is attempting to toggle status for user {TargetUserId}.",
                loggedInUserId, userId);

            var result = await _userRepository.ToggleUserStatusAsync(loggedInUserId, userId);

            if (!result.Status)
            {
                _logger.LogWarning("Failed to toggle status for user {TargetUserId} by admin {AdminId}. Reason: {Message}",
                    userId, loggedInUserId, result.Message);
                return BadRequest(result);
            }

            _logger.LogInformation("Status toggled successfully for user {TargetUserId} by admin {AdminId}.",
                userId, loggedInUserId);

            return Ok(result);
        }


    }
}
