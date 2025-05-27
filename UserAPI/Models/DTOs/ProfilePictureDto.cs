using Microsoft.AspNetCore.Mvc;

namespace UserAPI.Models.DTOs
{
    public class ProfilePictureDto
    {
        [FromForm]
        public IFormFile File { get; set; }
    }
}
