﻿namespace UserAPI.Models.DTOs
{
    public class UpdateUserDTO
    {
        
            

        public string UserName { get; set; }
        public string Email { get; set; }
        public string FitnessGoal { get; set; }
        public string Password { get; set; }
        public string? Role { get; set; }

    }
}
