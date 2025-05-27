namespace WorkoutService.Models.DTO
{
    public class GetAllUserDetailsDto
    {
        public int Id { get; set; }

        public string UserName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string? FitnessGoal { get; set; }

        public DateTime? CreatedAt { get; set; }

        public string Role { get; set; } = null!;
    }
}
