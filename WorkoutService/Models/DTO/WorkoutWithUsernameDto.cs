namespace WorkoutService.Models.DTO
{
    public class WorkoutWithUsernameDto
    {
        public int Id { get; set; }            // w.id
        public int UserId { get; set; }        // w.userId
        public string? Username { get; set; }   // u.Username
        public string ExerciseType { get; set; } // w.ExerciseType
        public int Duration { get; set; }      // w.Duration
        public int CaloriesBurned { get; set; } // w.CaloriesBurned
        public DateTime WorkoutDate { get; set; } // w.WorkoutDate
        public string? ImageUrl { get; set; }
    }
}
