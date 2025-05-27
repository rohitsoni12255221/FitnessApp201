namespace WorkoutService.Models;

public partial class Workout
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string ExerciseType { get; set; } = null!;

    public int Duration { get; set; }

    public int CaloriesBurned { get; set; }

    public DateTime WorkoutDate { get; set; }

    public bool IsActive { get; set; }

    public string? ImageUrl { get; set; }

    public virtual User Users { get; set; } = null!;
}
