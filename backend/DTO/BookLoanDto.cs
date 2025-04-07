using LibraryAPI.Models;

namespace LibraryAPI.DTO;

public class BookLoanDto
{
    public int Id { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;

     public int BookId { get; set; }
    public DateTime CheckoutDate { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsReturned { get; set; }

     public DateTime? ReturnDate { get; set; }

    public string? UserName { get; set; } = string.Empty;
    public string? UserEmail { get; set; } = string.Empty;

    public Book book { get; set; } = new Book();
}