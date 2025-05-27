using WorkoutService.Models;
using WorkoutService.Models.DTO;
using WorkoutService.Models.NewFolder;

namespace WorkoutService.Services.IInterfaces
{
    public interface IWorkout
    {
        Task<List<WorkoutWithUsernameDto>> GetAllWorkoutDetails();
        Task<Workout> GetWorkoutDetailsById(int id);
        Task<List<Workout>> GetWorkoutsDetailsByUserId(int userId);
        Task AddWorkout(AddWorkoutDto workout, string? imageUrl);
        Task UpdateWorkout(int id, UpdateWorkoutDto workout, string? imageUrl);
        Task DeleteWorkout(int id);
        Task DisableUserWorkoutsAsync(int userId);
        Task EnableUserWorkoutsAsync(int userId);
        Task<IEnumerable<Workout>> GetTodayWorkoutsByUserIdAsync(int userId);


    }
}
