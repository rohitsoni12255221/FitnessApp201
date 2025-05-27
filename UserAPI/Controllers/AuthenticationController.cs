using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Shared.Helpers;
using UserAPI.AuthRepo;
using UserAPI.IRepository;
using UserAPI.Models.DTOs;

namespace UserAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {

        private readonly JwtHelper _jwtHelper;
        private readonly IAuthenticationRepository _authRepo;
        private readonly ILogger<AuthenticationController> _logger; // Inject logger

        public AuthenticationController(
            IAuthenticationRepository authRepo,
            JwtHelper jwtHelper,
            ILogger<AuthenticationController> logger) // Inject here
        {
            _authRepo = authRepo;
            _jwtHelper = jwtHelper;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto request)
        {
            if (request == null)
            {
                _logger.LogWarning("Login attempt failed: request is null");
                return BadRequest(new { Status = false, Message = "Invalid request." });
            }

            var result = await _authRepo.ValidateUser(request.UserName, request.Password);

            if (!result.Status || result.Users == null)
            {
                _logger.LogWarning("Login failed for user: {Username}", request.UserName);
                return Unauthorized(new
                {
                    Status = false,
                    Message = result.Message
                });
            }

            var token = _jwtHelper.GenerateToken(result.Users.Id, result.Users.Role, result.Users.UserName);

            _logger.LogInformation("User logged in: {Username}", result.Users.UserName);

            return Ok(new
            {
                Status = true,
                Token = token,
                Message = result.Message
            });
        }


    }
}
