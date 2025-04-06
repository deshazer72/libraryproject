using Xunit;
using LibraryAPI.Services;
using LibraryAPI.Data;
using LibraryAPI.Models;
using LibraryAPI.DTO;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace LibraryAPI.Tests
{
    public class ReviewServiceTests
    {
        private readonly ReviewService _reviewService;

        public ReviewServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
            
            var context = new ApplicationDbContext(options);
            _reviewService = new ReviewService(context);
        }

        [Fact]
        public async Task GetAllReviewsAsync_ShouldReturnAllReviews()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_GetAll")
                .Options;

            // Clean up database
            using (var context = new ApplicationDbContext(options))
            {
                await context.Database.EnsureDeletedAsync();
                await context.Database.EnsureCreatedAsync();
                
                // Set up required entities
                var book = new Book { Id = 1, Title = "Test Book" };
                var user = new ApplicationUser { Id = "user1", UserName = "TestUser" };
                
                context.Books.Add(book);
                context.Users.Add(user);
                await context.SaveChangesAsync();

                // Add test reviews
                context.Reviews.Add(new Review 
                { 
                    Id = 1, 
                    Rating = 5, 
                    Comment = "Great book!", 
                    BookId = book.Id,
                    UserId = user.Id,
                    Book = book,
                    User = user
                });
                context.Reviews.Add(new Review 
                { 
                    Id = 2, 
                    Rating = 4, 
                    Comment = "Good read", 
                    BookId = book.Id,
                    UserId = user.Id,
                    Book = book,
                    User = user
                });
                await context.SaveChangesAsync();
            }

            // Use a fresh context for the test
            using (var context = new ApplicationDbContext(options))
            {
                var service = new ReviewService(context);

                // Act
                var result = await service.GetAllReviewsAsync();

                // Assert
                Assert.Equal(2, result.Count());
                Assert.All(result, review =>
                {
                    Assert.Equal("TestUser", review.UserName);
                    Assert.Equal(1, review.BookId);
                });
            }
        }

        [Fact]
        public async Task AddReviewAsync_ShouldAddNewReview()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_Add")
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                context.Books.Add(new Book { Id = 1, Title = "Test Book" });
                await context.SaveChangesAsync();

                var service = new ReviewService(context);
                var reviewDto = new ReviewDto
                {
                    BookId = 1,
                    Rating = 5,
                    Comment = "Excellent!",
                    UserId = "user1"
                };

                // Act
                var result = await service.AddReviewAsync(reviewDto);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(5, result.Rating);
                Assert.Equal("Excellent!", result.Comment);
            }
        }

        [Fact]
        public async Task UpdateReviewAsync_ShouldUpdateExistingReview()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_Update")
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                var review = new Review 
                { 
                    Id = 1, 
                    Rating = 3, 
                    Comment = "Original comment",
                    UserId = "user1",
                    BookId = 1
                };
                context.Reviews.Add(review);
                await context.SaveChangesAsync();

                var service = new ReviewService(context);
                var updateDto = new ReviewDto
                {
                    Id = 1,
                    Rating = 4,
                    Comment = "Updated comment",
                    UserId = "user1"
                };

                // Act
                var result = await service.UpdateReviewAsync(1, updateDto);

                // Assert
                Assert.Equal(4, result.Rating);
                Assert.Equal("Updated comment", result.Comment);
            }
        }
    }
}