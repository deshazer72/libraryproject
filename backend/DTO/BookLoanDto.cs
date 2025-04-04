public class BookLoanDto
{
    public int Id { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public DateTime CheckoutDate { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsReturned { get; set; }
}