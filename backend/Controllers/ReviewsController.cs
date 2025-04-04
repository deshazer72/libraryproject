using LibraryAPI.Data;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LibraryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ReviewsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Reviews/Book/5
    [HttpGet("Book/{bookId}")]
    public async Task<ActionResult<IEnumerable<Review>>> GetBookReviews(int bookId)
    {
        var reviews = await _context.Reviews
            .Include(r => r.User)
            .Where(r => r.BookId == bookId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return reviews;
    }

    // POST: api/Reviews
    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<Review>> CreateReview(ReviewDto reviewDto)
    {
        var book = await _context.Books.FindAsync(reviewDto.BookId);
        if (book == null)
        {
            return NotFound("Book not found");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Check if user has already reviewed this book
        var existingReview = await _context.Reviews
            .FirstOrDefaultAsync(r => r.BookId == reviewDto.BookId && r.UserId == userId);

        if (existingReview != null)
        {
            return BadRequest("You have already reviewed this book");
        }

        var review = new Review
        {
            BookId = reviewDto.BookId,
            UserId = userId,
            Rating = reviewDto.Rating,
            Comment = reviewDto.Comment,
            CreatedAt = DateTime.UtcNow
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        // Load the user for the response
        await _context.Entry(review).Reference(r => r.User).LoadAsync();

        return CreatedAtAction("GetBookReviews", new { bookId = review.BookId }, review);
    }

    // PUT: api/Reviews/5
    [HttpPut("{id}")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> UpdateReview(int id, ReviewDto reviewDto)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review == null)
        {
            return NotFound();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (review.UserId != userId)
        {
            return Forbid();
        }

        review.Rating = reviewDto.Rating;
        review.Comment = reviewDto.Comment;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ReviewExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // DELETE: api/Reviews/5
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review == null)
        {
            return NotFound();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Librarian");

        // Only allow the review author or librarians to delete a review
        if (review.UserId != userId && !isAdmin)
        {
            return Forbid();
        }

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ReviewExists(int id)
    {
        return _context.Reviews.Any(e => e.Id == id);
    }
}

public class ReviewDto
{
    public int BookId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}