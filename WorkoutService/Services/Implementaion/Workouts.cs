using Microsoft.EntityFrameworkCore;
using WorkoutService.Models;
using WorkoutService.Models.DTO;
using WorkoutService.Models.NewFolder;
using WorkoutService.Services.IInterfaces;

namespace WorkoutService.Services.Implementaion
{
    public class Workouts : IWorkout
    {
        private readonly WorkoutDbContext _context;

        public Workouts(WorkoutDbContext context)
        {
            _context = context;
        }

        public async Task<List<WorkoutWithUsernameDto>> GetAllWorkoutDetails()
        {   var workouts = await _context.Set<WorkoutWithUsernameDto>()
                .FromSqlRaw("EXEC GetAllWorkoutDetails")
                .AsNoTracking()
                .ToListAsync();

            return workouts;
        }

        public async Task<Models.Workout> GetWorkoutDetailsById(int id)
        {
            var result = await _context.Workouts.FromSqlRaw("EXEC GetWorkoutDetailsById @Id = {0}", id).ToListAsync();
            return result.FirstOrDefault();
        }

        public async Task<List<Models.Workout>> GetWorkoutsDetailsByUserId(int userId)
        {
            return await _context.Workouts.FromSqlRaw("EXEC GetWorkoutsDetailsByUserId @UserId = {0}", userId).ToListAsync();
        }

        public async Task AddWorkout(AddWorkoutDto workout, string? imageUrl)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($@"
        EXEC AddWorkout 
            @UserId = {workout.UserId}, 
            @ExerciseType = {workout.ExerciseType}, 
            @Duration = {workout.Duration}, 
            @CaloriesBurned = {workout.CaloriesBurned},
            @ImageUrl = {imageUrl}");
        }



        public async Task UpdateWorkout(int id, UpdateWorkoutDto workout, string? imageUrl)
        {
            var rowsAffected = await _context.Database.ExecuteSqlInterpolatedAsync($@"
        EXEC UpdateWorkout 
            @Id = {id}, 
            @ExerciseType = {workout.ExerciseType}, 
            @Duration = {workout.Duration}, 
            @CaloriesBurned = {workout.CaloriesBurned},
            @ImageUrl = {imageUrl}
    ");

            if (rowsAffected == 0)
                throw new Exception("Update failed or workout not found.");
        }



        public async Task DeleteWorkout(int id)
        {
            await _context.Database.ExecuteSqlRawAsync("EXEC DeleteWorkout @Id = {0}", id);
        }

        public async Task DisableUserWorkoutsAsync(int userId)
        {


            var workouts = await _context.Workouts
                .Where(w => w.UserId == userId && w.IsActive)
                .ToListAsync();

            foreach (var workout in workouts)
            {
                workout.IsActive = false;
            }

            await _context.SaveChangesAsync();
        }

        public async Task EnableUserWorkoutsAsync(int userId)
        {
            var workouts = await _context.Workouts
                .Where(w => w.UserId == userId && !w.IsActive)
                .ToListAsync();

            foreach (var workout in workouts)
            {
                workout.IsActive = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Workout>> GetTodayWorkoutsByUserIdAsync(int userId)
        {
            return await _context.Workouts
                .FromSqlRaw("EXEC sp_GetTodayWorkoutByUserId @UserId={0}", userId)
                .ToListAsync();
        }

    }
}
