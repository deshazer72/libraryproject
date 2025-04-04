using Bogus;
using LibraryAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Data.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedBooksAsync(ApplicationDbContext context)
    {
        // Only seed if there are no books in the database
        if (await context.Books.AnyAsync())
        {
            return;
        }

        var bookCategories = new[] { "Fiction", "Non-Fiction", "Science Fiction", "Fantasy", "Mystery", "Romance", "Thriller", "Biography", "History", "Science", "Technology" };

        // Configure Bogus faker for books
        var faker = new Faker<Book>()
            .RuleFor(b => b.Title, f => f.Commerce.ProductName())
            .RuleFor(b => b.Author, f => f.Name.FullName())
            .RuleFor(b => b.Description, f => f.Lorem.Paragraphs(3))
            .RuleFor(b => b.CoverImageUrl, f => f.Image.PicsumUrl(width: 300, height: 400))
            .RuleFor(b => b.Publisher, f => f.Company.CompanyName())
            .RuleFor(b => b.PublicationDate, f => f.Date.Past(10))
            .RuleFor(b => b.Category, f => f.PickRandom(bookCategories))
            .RuleFor(b => b.ISBN, f => f.Commerce.Ean13())
            .RuleFor(b => b.PageCount, f => f.Random.Number(100, 800))
            .RuleFor(b => b.IsAvailable, f => true);

        // Generate 50 books
        var books = faker.Generate(50);
        
        await context.Books.AddRangeAsync(books);
        await context.SaveChangesAsync();
    }
}