using LibraryAPI.DTO;

public interface IBooks
{
    Task<IEnumerable<BookDto>> GetFeaturedBooksAsync();
    Task<IEnumerable<BookDto>> GetAllBooksAsync(string? searchTerm = null, string? sortBy = null, bool? availableOnly = null);
    Task<BookDto> GetBookByIdAsync(int id);
    Task<BookDto> AddBookAsync(BookDto bookDto);
    Task<BookDto> UpdateBookAsync(int id, BookDto bookDto);
    Task DeleteBookAsync(int id);
    Task<bool> IsBookAvailableAsync(int bookId);
}