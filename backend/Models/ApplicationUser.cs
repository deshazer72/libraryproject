using Microsoft.AspNetCore.Identity;

namespace LibraryAPI.Models;

public class ApplicationUser : IdentityUser
{
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    public virtual ICollection<BookLoan> BookLoans { get; set; } = new List<BookLoan>();
}