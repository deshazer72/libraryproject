using Xunit;
using LibraryAPI.Services;
using LibraryAPI.Data;
using LibraryAPI.Models;
using LibraryAPI.DTO;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Tests
{
    public class BookServiceTests
    {
        [Fact]
        public async Task GetFeaturedBooksAsync_ShouldReturnUpToSixBooks()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_Featured")
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                // Add 8 books
                for (int i = 1; i <= 8; i++)
                {
                    context.Books.Add(new Book 
                    { 
                        Id = i,
                        Title = $"Book {i}",
                        Author = $"Author {i}",
                        ISBN = $"ISBN{i}"
                    });
                }
                await context.SaveChangesAsync();

                var service = new BookService(context);

                // Act
                var result = await service.GetFeaturedBooksAsync();

                // Assert
                Assert.True(result.Count() <= 6);
            }
        }

        [Fact]
        public async Task GetBookByIdAsync_ShouldReturnCorrectBook()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_GetById")
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                var book = new Book
                {
                    Id = 1,
                    Title = "Test Book",
                    Author = "Test Author",
                    ISBN = "1234567890"
                };
                context.Books.Add(book);
                await context.SaveChangesAsync();

                var service = new BookService(context);

                // Act
                var result = await service.GetBookByIdAsync(1);

                // Assert
                Assert.NotNull(result);
                Assert.Equal("Test Book", result.Title);
                Assert.Equal("Test Author", result.Author);
            }
        }

        [Fact]
        public async Task AddBookAsync_ShouldAddNewBook()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_AddBook")
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                var service = new BookService(context);
                var bookDto = new BookDto
                {
                    Title = "New Book",
                    Author = "New Author",
                    ISBN = "0987654321"
                };

                // Act
                var result = await service.AddBookAsync(bookDto);

                // Assert
                Assert.NotNull(result);
                var savedBook = await context.Books.FirstOrDefaultAsync(b => b.Title == "New Book");
                Assert.NotNull(savedBook);
                Assert.Equal("New Author", savedBook.Author);
            }
        }

        [Fact]
        public async Task UpdateBookAsync_ShouldUpdateExistingBook()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_UpdateBook")
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                var book = new Book
                {
                    Id = 1,
                    Title = "Original Title",
                    Author = "Original Author",
                    ISBN = "1111111111"
                };
                context.Books.Add(book);
                await context.SaveChangesAsync();

                var service = new BookService(context);
                var updateDto = new BookDto
                {
                    Id = 1,
                    Title = "Updated Title",
                    Author = "Updated Author",
                    ISBN = "1111111111"
                };

                // Act
                var result = await service.UpdateBookAsync(1, updateDto);

                // Assert
                Assert.Equal("Updated Title", result.Title);
                Assert.Equal("Updated Author", result.Author);
            }
        }
    }
}