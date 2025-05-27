
using Microsoft.EntityFrameworkCore;
using WorkoutService.Models.DTO;

namespace WorkoutService.Models;

public partial class WorkoutDbContext : DbContext
{
    public WorkoutDbContext()
    {
    }

    public WorkoutDbContext(DbContextOptions<WorkoutDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Workout> Workouts { get; set; }
    public DbSet<WorkoutWithUsernameDto> WorkoutWithUsernameDtos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Workout>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Workouts__3213E83FD2908E6D");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CaloriesBurned).HasColumnName("caloriesBurned");
            entity.Property(e => e.Duration).HasColumnName("duration");
            entity.Property(e => e.ExerciseType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("exerciseType");
            entity.Property(e => e.UserId).HasColumnName("userId");
            entity.Property(e => e.WorkoutDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("workoutDate");
        });

        OnModelCreatingPartial(modelBuilder);

        modelBuilder.Entity<WorkoutWithUsernameDto>()
    .HasNoKey()
    .ToView(null);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
