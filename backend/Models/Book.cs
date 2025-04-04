using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Models;

public class Book
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Author { get; set; } = string.Empty;

    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string CoverImageUrl { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Publisher { get; set; } = string.Empty;

    [Required]
    public DateTime PublicationDate { get; set; }

    [Required]
    [StringLength(50)]
    public string Category { get; set; } = string.Empty;

    [Required]
    [StringLength(13)]
    public string ISBN { get; set; } = string.Empty;

    public int PageCount { get; set; }

    public bool IsAvailable { get; set; } = true;

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    
    public virtual ICollection<BookLoan> BookLoans { get; set; } = new List<BookLoan>();

    public double AverageRating => Reviews.Any() ? Reviews.Average(r => r.Rating) : 0;
}