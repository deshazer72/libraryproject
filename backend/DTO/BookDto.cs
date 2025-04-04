public class BookDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CoverImageUrl { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public DateTime PublicationDate { get; set; }
    public string Category { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public int PageCount { get; set; }
    public bool IsAvailable { get; set; }
    public double AverageRating { get; set; }

     public int Rating { get; set; }

      public string Comment { get; set; } = string.Empty;
}