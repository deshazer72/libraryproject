using LibraryAPI.Models;
using LibraryAPI.DTO;
using LibraryAPI.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace LibraryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookLoansController : ControllerBase
{
    private readonly IBookLoans _bookLoansService;
    private readonly IHubContext<LibraryHub> _hubContext;

    public BookLoansController(IBookLoans bookLoansService, IHubContext<LibraryHub> hubContext)
    {
        _bookLoansService = bookLoansService;
        _hubContext = hubContext;
    }

    // GET: api/BookLoans
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookLoanDto>>> GetMyBookLoans()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return BadRequest("User ID not found");
        }
        
        try 
        {
            var bookLoans = await _bookLoansService.GetMyBookLoansAsync(userId);
            return Ok(bookLoans);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // GET: api/BookLoans/All
    [HttpGet("All")]
    [Authorize(Roles = "Librarian")]
    public async Task<ActionResult<IEnumerable<BookLoanDto>>> GetAllBookLoans()
    {
        try
        {
            var bookLoans = await _bookLoansService.GetAllBookLoansAsync();
            return Ok(bookLoans);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // POST: api/BookLoans/checkout/5
    [HttpPost("checkout/{bookId}")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<BookLoan>> CheckoutBook(int bookId)
    {
        try
        {
            var bookLoan = await _bookLoansService.AddBookLoanAsync(bookId);
            
            // Send notification to librarians
            await _hubContext.Clients.Group("Librarians")
                .SendAsync("ReceiveNotification", $"New book loan: Book ID {bookId} has been checked out");

            return CreatedAtAction(nameof(GetMyBookLoans), null, bookLoan);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PUT: api/BookLoans/return/5
    [HttpPut("return/{loanId}")]
    [Authorize(Roles = "Librarian")]
    public async Task<IActionResult> ReturnBook(int loanId)
    {
        try
        {
            var bookLoanDto = new BookLoanDto { Id = loanId, IsReturned = true };
            var updatedLoan = await _bookLoansService.UpdateBookLoanAsync(loanId, bookLoanDto);

            // Get the loan details and notify the user using their email
            if (updatedLoan != null && !string.IsNullOrEmpty(updatedLoan.UserEmail))
            {
                await _hubContext.Clients.User(updatedLoan.UserEmail)
                    .SendAsync("ReceiveNotification", $"Your book '{updatedLoan.BookTitle}' has been marked as returned. Thank you!");
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }
}