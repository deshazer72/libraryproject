using LibraryAPI.Data;
using LibraryAPI.DTO;
using LibraryAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Services;

public class BookService : IBooks
{
    private readonly ApplicationDbContext _context;

    public BookService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<BookDto>> GetFeaturedBooksAsync()
    {
        var totalBooks = await _context.Books.CountAsync();
        var take = Math.Min(totalBooks, 6); // Display up to 6 random books
        
        return await _context.Books
            .Include(b => b.Reviews)
            .OrderBy(_ => Guid.NewGuid()) // Random ordering
            .Take(take)
            .Select(b => new BookDto
            {
                Id = b.Id,
                Title = b.Title,
                Author = b.Author,
                Description = b.Description,
                CoverImageUrl = b.CoverImageUrl,
                Publisher = b.Publisher,
                PublicationDate = b.PublicationDate,
                Category = b.Category,
                ISBN = b.ISBN,
                PageCount = b.PageCount,
                IsAvailable = b.IsAvailable,
                AverageRating = b.Reviews.Any() ? b.Reviews.Average(r => r.Rating) : 0.0
            })
            .ToListAsync();;
    }

    public async Task<BookDto> GetBookByIdAsync(int id)
    {
        var book = await _context.Books
            .Include(b => b.Reviews)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book == null)
        {
            throw new InvalidOperationException("Book not found");
        }

        return new BookDto
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            Description = book.Description,
            CoverImageUrl = book.CoverImageUrl,
            Publisher = book.Publisher,
            PublicationDate = book.PublicationDate,
            Category = book.Category,
            ISBN = book.ISBN,
            PageCount = book.PageCount,
            IsAvailable = book.IsAvailable,
            AverageRating = book.Reviews.Any() ? book.Reviews.Average(r => r.Rating) : 0.0
        };
    }

    public async Task<BookDto> AddBookAsync(BookDto bookDto)
    {
        var book = new Book
        {
            Title = bookDto.Title,
            Author = bookDto.Author,
            Description = bookDto.Description,
            CoverImageUrl = bookDto.CoverImageUrl,
            Publisher = bookDto.Publisher,
            PublicationDate = bookDto.PublicationDate,
            Category = bookDto.Category,
            ISBN = bookDto.ISBN,
            PageCount = bookDto.PageCount,
            IsAvailable = true
        };

        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        bookDto.Id = book.Id;
        return bookDto;
    }

    public async Task<BookDto> UpdateBookAsync(int id, BookDto bookDto)
    {
        var existingBook = await _context.Books.FindAsync(id);
        if (existingBook == null)
        {
            throw new InvalidOperationException("Book not found");
        }

        existingBook.Title = bookDto.Title;
        existingBook.Author = bookDto.Author;
        existingBook.Description = bookDto.Description;
        existingBook.CoverImageUrl = bookDto.CoverImageUrl;
        existingBook.Publisher = bookDto.Publisher;
        existingBook.PublicationDate = bookDto.PublicationDate;
        existingBook.Category = bookDto.Category;
        existingBook.ISBN = bookDto.ISBN;
        existingBook.PageCount = bookDto.PageCount;

        await _context.SaveChangesAsync();
        return bookDto;
    }

    public async Task DeleteBookAsync(int id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book == null)
        {
            throw new InvalidOperationException("Book not found");
        }

        // Check if the book is currently loaned
        var activeLoans = await _context.BookLoans
            .AnyAsync(l => l.BookId == id && !l.ReturnDate.HasValue);

        if (activeLoans)
        {
            throw new InvalidOperationException("Cannot delete a book that is currently on loan.");
        }

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsBookAvailableAsync(int bookId)
    {
        var book = await _context.Books.FindAsync(bookId);
        return book?.IsAvailable ?? false;
    }

    // Additional method not in interface
    public async Task<IEnumerable<BookDto>> GetAllBooksAsync(string? searchTerm = null, string? sortBy = null, bool? availableOnly = null)
    {
        IQueryable<Book> query = _context.Books
            .Include(b => b.Reviews);

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(b => b.Title.Contains(searchTerm) || b.Author.Contains(searchTerm));
        }

        // Apply availability filter
        if (availableOnly.HasValue && availableOnly.Value)
        {
            query = query.Where(b => b.IsAvailable);
        }

        // Apply sorting
        switch (sortBy?.ToLower())
        {
            case "title":
                query = query.OrderBy(b => b.Title);
                break;
            case "author":
                query = query.OrderBy(b => b.Author);
                break;
            case "rating":
                query = query.OrderByDescending(b => b.AverageRating);
                break;
            default:
                query = query.OrderBy(b => b.Title);
                break;
        }

        return await query
            .Select(b => new BookDto
            {
                Id = b.Id,
                Title = b.Title,
                Author = b.Author,
                Description = b.Description,
                CoverImageUrl = b.CoverImageUrl,
                Publisher = b.Publisher,
                PublicationDate = b.PublicationDate,
                Category = b.Category,
                ISBN = b.ISBN,
                PageCount = b.PageCount,
                IsAvailable = b.IsAvailable,
                AverageRating = b.Reviews.Any() ? b.Reviews.Average(r => r.Rating) : 0.0
            })
            .ToListAsync();
    }
}