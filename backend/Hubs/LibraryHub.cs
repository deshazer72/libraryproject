using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace LibraryAPI.Hubs;

[Authorize]
public class LibraryHub : Hub
{
    public async Task SendBookLoanNotification(string userId, string message)
    {
        await Clients.User(userId).SendAsync("ReceiveNotification", message);
    }

    public async Task SendLibrarianNotification(string message)
    {
        await Clients.Group("Librarians").SendAsync("ReceiveNotification", message);
    }

    public override async Task OnConnectedAsync()
    {
        var user = Context.User;
        if (user?.IsInRole("Librarian") == true)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Librarians");
        }
        await base.OnConnectedAsync();
    }
}