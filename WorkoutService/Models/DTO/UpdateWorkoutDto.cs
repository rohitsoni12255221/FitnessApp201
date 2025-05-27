namespace WorkoutService.Models.NewFolder
{
    public class UpdateWorkoutDto
    {
        public int Id { get; set; }

        public string ExerciseType { get; set; } = null!;

        public int Duration { get; set; }

        public int CaloriesBurned { get; set; }
        public IFormFile? NewImageUrl { get; set; }
        public string? ExistingImageUrl { get; set; }
    }
}
