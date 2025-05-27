using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkoutService.Models;
using WorkoutService.Models.NewFolder;
using WorkoutService.Services.IInterfaces;
using WorkoutService.Services.Implementaion;

namespace WorkoutService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkoutController : ControllerBase
    {
        private readonly IWorkout _repo;
        private readonly BlobService _blobService;
        public WorkoutController(IWorkout repo, BlobService blobService)
        {
            _repo = repo;
            _blobService = blobService;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("GetAllWorkout")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var workouts = await _repo.GetAllWorkoutDetails();
                return Ok(workouts);
            }
            catch(Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [Authorize(Roles = "Admin,Id")]
        [HttpGet("GetWorkoutDetailById/{id}")]
        //[Authorize]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var workout = await _repo.GetWorkoutDetailsById(id);
                return workout == null ? NotFound() : Ok(workout);
            }
            catch(Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [Authorize(Roles = "Admin,User")]
        [HttpGet("GetWorkoutDetailByUserId/{id}")]
        //[Authorize]
        public async Task<IActionResult> GetByUserId(int id)
        {
            try
            {
                var workout = await _repo.GetWorkoutsDetailsByUserId(id);
                return workout == null ? NotFound() : Ok(workout);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }

        }

        //[Authorize(Roles = "Admin,User")]
        //[HttpPost("AddWorkoutDetails")]
        //public async Task<IActionResult> Add([FromBody] AddWorkoutDto workout)
        //{
        //    try
        //    {
        //        await _repo.AddWorkout(workout);
        //        return Ok( new { Message = "Workout added Successfully!" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { error = ex.Message });
        //    }
        //}
        [Authorize(Roles = "Admin,User")]
        [HttpPost("AddWorkoutDetails")]
        public async Task<IActionResult> Add([FromForm] AddWorkoutDto workout)
        {
            try
            {
                string? imageUrl = null;

                // Upload image if provided
                if (workout.ImageUrl != null)
                {
                    string fileName = $"{Guid.NewGuid()}{Path.GetExtension(workout.ImageUrl.FileName)}";
                    imageUrl = await _blobService.UploadAsync(workout.ImageUrl, fileName);
                }

                // Pass the URL to the repo
                await _repo.AddWorkout(workout, imageUrl);

                return Ok(new { Message = "Workout added successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.ToString() });
            }
        }

        [Authorize(Roles = "Admin,User")]
        [HttpPut("UpdateWorkoutDetail/{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateWorkoutDto workout)
        {
            try
            {
                string? imageUrl = null;

                if (workout.NewImageUrl != null)
                {
                    // New image uploaded, upload to blob storage
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(workout.NewImageUrl.FileName);
                    imageUrl = await _blobService.UploadAsync(workout.NewImageUrl, uniqueFileName);
                }
                else if (!string.IsNullOrEmpty(workout.ExistingImageUrl))
                {
                    // No new image, keep existing image URL
                    imageUrl = workout.ExistingImageUrl;
                }
                // Else imageUrl remains null - you might want to handle if image is optional or required

                await _repo.UpdateWorkout(id, workout, imageUrl);

                return Ok(new { Message = "Workout updated successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }




        [Authorize(Roles = "Admin,User")]
        [HttpDelete("DeleteWorkoutDetail/{id}")]
        //[Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _repo.DeleteWorkout(id);
                return Ok(new { Message = "Workout deleted Successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
           
        }

        [HttpGet("today-workouts/{id}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> GetTodayWorkouts(int id)
        {
            try
            {
                var workouts = await _repo.GetTodayWorkoutsByUserIdAsync(id);
                return Ok(workouts);
            }
            catch(Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            
        }

    }
}
