using LibraryAPI.DTO;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LibraryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviews _reviewService;

    public ReviewsController(IReviews reviewService)
    {
        _reviewService = reviewService;
    }

    // GET: api/Reviews/Book/5
    [HttpGet("Book/{bookId}")]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetBookReviews(int bookId)
    {
        try 
        {
            var reviews = await _reviewService.GetAllReviewsAsync();
            return Ok(reviews.Where(r => r.BookId == bookId));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // POST: api/Reviews
    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<ReviewDto>> CreateReview(ReviewDto reviewDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        reviewDto.UserId = userId;

        try
        {
            var review = await _reviewService.AddReviewAsync(reviewDto);
            return CreatedAtAction(nameof(GetBookReviews), new { bookId = review.BookId }, review);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PUT: api/Reviews/5
    [HttpPut("{id}")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> UpdateReview(int id, ReviewDto reviewDto)
    {
        if (id != reviewDto.Id)
        {
            return BadRequest();
        }

        reviewDto.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        try
        {
            await _reviewService.UpdateReviewAsync(id, reviewDto);
            return NoContent();
        }
        catch (InvalidOperationException ex) when (ex.Message == "Review not found")
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // DELETE: api/Reviews/5
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isLibrarian = User.IsInRole("Librarian");

        try
        {
            var review = await _reviewService.GetReviewByIdAsync(id);
            
            if (review.UserId != userId && !isLibrarian)
            {
                return Forbid();
            }

            await _reviewService.DeleteReviewAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }
}