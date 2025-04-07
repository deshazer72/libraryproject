using LibraryAPI.DTO;
using LibraryAPI.Models;

public interface IBookLoans
{
    Task<IEnumerable<BookLoanDto>> GetAllBookLoansAsync();

    Task<IEnumerable<BookLoanDto>> GetMyBookLoansAsync(string userId);
    Task<BookLoanDto> GetBookLoanByIdAsync(int id);
    Task<BookLoanDto> AddBookLoanAsync(int bookId);
    Task<BookLoanDto> UpdateBookLoanAsync(int id, BookLoanDto bookLoanDto);
    Task DeleteBookLoanAsync(int id);
    Task<bool> IsBookAvailableAsync(int bookId);
}