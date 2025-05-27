using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserAPI.DAL;
using Shared.Helpers;
using System.Data;
using UserAPI.Models.DTOs;

namespace UserAPI.IRepository
{
    public class AuthenticationRepository : IAuthenticationRepository
    {
        private readonly UserDbContext _context;
        private readonly JwtHelper _jwtHelper;

        public AuthenticationRepository(UserDbContext context)
        {
            _context = context;

        }

        public async Task<ResponseDto?> ValidateUser(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return new ResponseDto
                {
                    Status = false,
                    Message = "Invalid username or password.",
                };
            }

            if (!user.IsActive)
            {
                return new ResponseDto
                {
                    Status = false,
                    Message = "Account is deactivated. Please contact the administrator.",
                };
            }

            return new ResponseDto
            {
                Status = true,
                Message = "Login successful.",
                Users = user
            };


        }
    }
}
