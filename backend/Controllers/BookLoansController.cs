using LibraryAPI.Models;
using LibraryAPI.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LibraryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookLoansController : ControllerBase
{
    private readonly IBookLoans _bookLoansService;

    public BookLoansController(IBookLoans bookLoansService)
    {
        _bookLoansService = bookLoansService;
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
            await _bookLoansService.UpdateBookLoanAsync(loanId, bookLoanDto);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }
}