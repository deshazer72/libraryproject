using Xunit;
using Moq;
using LibraryAPI.Services;
using LibraryAPI.Data;
using LibraryAPI.Models;
using LibraryAPI.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace LibraryAPI.Tests
{
    public class BookLoanServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;

        public BookLoanServiceTests()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var options = new Mock<IOptions<IdentityOptions>>();
            var userValidators = new List<IUserValidator<ApplicationUser>>();
            var passwordValidators = new List<IPasswordValidator<ApplicationUser>>();
            var keyNormalizer = new UpperInvariantLookupNormalizer();
            var errors = new IdentityErrorDescriber();
            var services = new Mock<IServiceProvider>();
            var logger = new Mock<ILogger<UserManager<ApplicationUser>>>();

            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object,
                options.Object,
                new PasswordHasher<ApplicationUser>(),
                userValidators,
                passwordValidators,
                keyNormalizer,
                errors,
                services.Object,
                logger.Object);
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        }

        [Fact]
        public async Task GetAllBookLoansAsync_ShouldReturnAllLoans()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_GetAllLoans")
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                // Add test data
                var user = new ApplicationUser { Id = "user1", UserName = "testuser" };
                var book = new Book { Id = 1, Title = "Test Book" };
                context.Users.Add(user);
                context.Books.Add(book);
                context.BookLoans.Add(new BookLoan 
                { 
                    Id = 1,
                    BookId = 1,
                    UserId = "user1",
                    CheckoutDate = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(14)
                });
                await context.SaveChangesAsync();

                var service = new BookLoanService(context, _mockUserManager.Object, _mockHttpContextAccessor.Object);

                // Act
                var result = await service.GetAllBookLoansAsync();

                // Assert
                Assert.Single(result);
            }
        }

        [Fact]
        public async Task AddBookLoanAsync_ShouldCreateNewLoan()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_AddLoan")
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                var user = new ApplicationUser { Id = "user1", UserName = "testuser" };
                var book = new Book { Id = 1, Title = "Test Book", IsAvailable = true };
                context.Users.Add(user);
                context.Books.Add(book);
                await context.SaveChangesAsync();

                // Setup HttpContext with user claims
                var httpContext = new DefaultHttpContext();
                var claims = new System.Security.Claims.ClaimsPrincipal(
                    new System.Security.Claims.ClaimsIdentity(
                        new System.Security.Claims.Claim[] {
                            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "user1")
                        }));
                httpContext.User = claims;
                _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

                var service = new BookLoanService(context, _mockUserManager.Object, _mockHttpContextAccessor.Object);

                // Act
                var result = await service.AddBookLoanAsync(1);

                // Assert
                Assert.NotNull(result);
                Assert.Equal("user1", result.UserId);
                Assert.Equal(1, result.BookId);
            }
        }

        [Fact]
        public async Task GetMyBookLoansAsync_ShouldReturnUserLoans()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_MyLoans")
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                var user = new ApplicationUser { Id = "user1", UserName = "testuser" };
                var book = new Book { Id = 1, Title = "Test Book" };
                context.Users.Add(user);
                context.Books.Add(book);
                context.BookLoans.Add(new BookLoan
                {
                    Id = 1,
                    BookId = 1,
                    UserId = "user1",
                    CheckoutDate = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(14)
                });
                await context.SaveChangesAsync();

                var service = new BookLoanService(context, _mockUserManager.Object, _mockHttpContextAccessor.Object);

                // Act
                var result = await service.GetMyBookLoansAsync("user1");

                // Assert
                Assert.Single(result);
                Assert.Equal("Test Book", result.First().BookTitle);
            }
        }

        [Fact]
        public async Task UpdateBookLoanAsync_ShouldReturnBookWhenReturned()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_ReturnBook")
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                var user = new ApplicationUser { Id = "user1", UserName = "testuser" };
                var book = new Book { Id = 1, Title = "Test Book", IsAvailable = false };
                var loan = new BookLoan
                {
                    Id = 1,
                    BookId = 1,
                    UserId = "user1",
                    CheckoutDate = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(14)
                };

                context.Users.Add(user);
                context.Books.Add(book);
                context.BookLoans.Add(loan);
                await context.SaveChangesAsync();

                var service = new BookLoanService(context, _mockUserManager.Object, _mockHttpContextAccessor.Object);
                var updateDto = new BookLoanDto
                {
                    Id = 1,
                    IsReturned = true
                };

                // Act
                var result = await service.UpdateBookLoanAsync(1, updateDto);

                // Assert
                Assert.True(result.IsReturned);
                var bookLoan = await context.BookLoans.FindAsync(1);
                Assert.NotNull(bookLoan?.ReturnDate);
                var updatedBook = await context.Books.FindAsync(1);
                Assert.NotNull(updatedBook);
                Assert.True(updatedBook.IsAvailable);
            }
        }
    }
}