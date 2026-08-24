using AppointmentApi.DTOs;
using AppointmentApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AppointmentApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly ReviewService _reviewService;

        public ReviewsController(ReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [Authorize(Roles = "Customer")]
        [HttpPost]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto dto)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Unauthorized("Kullanıcı kimliği doğrulanamadı.");
            }

            var review = await _reviewService.CreateAsync(dto, userId);

            return CreatedAtAction(nameof(GetStaffReviews), new { staffId = review.StaffId }, review);
        }

        [AllowAnonymous]
        [HttpGet("staff/{staffId}")]
        public async Task<IActionResult> GetStaffReviews(int staffId)
        {
            var reviews = await _reviewService.GetStaffReviewsAsync(staffId);
            return Ok(reviews);
        }
    }
}
