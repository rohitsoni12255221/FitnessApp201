namespace WorkoutService.Models.NewFolder
{
    public class AddWorkoutDto
    {
        public int UserId { get; set; }

        public string ExerciseType { get; set; } = null!;

        public int Duration { get; set; }

        public int CaloriesBurned { get; set; }
        public IFormFile? ImageUrl { get; set; }
    }
}
