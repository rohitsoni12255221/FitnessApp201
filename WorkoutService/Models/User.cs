using System;
using System.Collections.Generic;

namespace WorkoutService.Models;

public partial class User
{
    public int Id { get; set; }

    public string UserName { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? FitnessGoal { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string Role { get; set; } = null!;

    public string? OtpCode { get; set; }

    public DateTime? OtpExpiry { get; set; }

    public string? ProfileUrl { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Workout> Workouts { get; set; } = new List<Workout>();
}
