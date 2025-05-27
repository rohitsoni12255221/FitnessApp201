using UserAPI.Models.DTOs;

namespace UserAPI.IRepository
{
    public interface IAuthenticationRepository
    {
        Task<ResponseDto?> ValidateUser(string username, string password);
    }
}
