using LibraryAPI.Data;
using LibraryAPI.DTO;
using LibraryAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Services;

public class ReviewService : IReviews
{
    private readonly ApplicationDbContext _context;

    public ReviewService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ReviewDto>> GetAllReviewsAsync()
    {
        return await _context.Reviews
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewDto
            {
                Id = r.Id,
                BookId = r.BookId,
                Rating = r.Rating,
                Comment = r.Comment ?? string.Empty,
                UserName = r.User != null ? r.User.UserName ?? "Unknown User" : "Unknown User",
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<ReviewDto> GetReviewByIdAsync(int id)
    {
        var review = await _context.Reviews
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (review == null)
        {
            throw new InvalidOperationException("Review not found");
        }

        return new ReviewDto
        {
            Id = review.Id,
            BookId = review.BookId,
            Rating = review.Rating,
            Comment = review.Comment ?? string.Empty,
            UserName = review.User?.UserName ?? "Unknown User",
            CreatedAt = review.CreatedAt
        };
    }

    public async Task<ReviewDto> AddReviewAsync(ReviewDto reviewDto)
    {
        var book = await _context.Books.FindAsync(reviewDto.BookId);
        if (book == null)
        {
            throw new InvalidOperationException("Book not found");
        }

        // Check if user has already reviewed this book
        var existingReview = await _context.Reviews
            .FirstOrDefaultAsync(r => r.BookId == reviewDto.BookId && r.UserId == reviewDto.UserId);

        if (existingReview != null)
        {
            throw new InvalidOperationException("You have already reviewed this book");
        }

        var review = new Review
        {
            BookId = reviewDto.BookId,
            UserId = reviewDto.UserId ?? throw new InvalidOperationException("User ID is required"),
            Rating = reviewDto.Rating,
            Comment = reviewDto.Comment ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        // Load the user for the response
        await _context.Entry(review).Reference(r => r.User).LoadAsync();

        reviewDto.Id = review.Id;
        reviewDto.UserName = review.User?.UserName ?? "Unknown User";
        reviewDto.CreatedAt = review.CreatedAt;

        return reviewDto;
    }

    public async Task<ReviewDto> UpdateReviewAsync(int id, ReviewDto reviewDto)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review == null)
        {
            throw new InvalidOperationException("Review not found");
        }

        if (review.UserId != reviewDto.UserId)
        {
            throw new InvalidOperationException("You can only update your own reviews");
        }

        review.Rating = reviewDto.Rating;
        review.Comment = reviewDto.Comment ?? string.Empty;

        await _context.SaveChangesAsync();

        return reviewDto;
    }

    public async Task DeleteReviewAsync(int id)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review == null)
        {
            throw new InvalidOperationException("Review not found");
        }

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();
    }
}