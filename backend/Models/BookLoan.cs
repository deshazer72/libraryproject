using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Models;

public class BookLoan
{
    public int Id { get; set; }

    public int BookId { get; set; }
    public virtual Book Book { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;
    public virtual ApplicationUser User { get; set; } = null!;

    public DateTime CheckoutDate { get; set; } = DateTime.UtcNow;
    
    public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(5);

    public DateTime? ReturnDate { get; set; }

    public bool IsReturned => ReturnDate.HasValue;
}