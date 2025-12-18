using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SupportDeskBE.Hubs
{
    [Authorize] // requires JWT
    public class TicketHub : Hub
    {
        public Task JoinTicket(string ticketId) =>
            Groups.AddToGroupAsync(Context.ConnectionId, $"ticket:{ticketId}");

        public Task LeaveTicket(string ticketId) =>
            Groups.RemoveFromGroupAsync(Context.ConnectionId, $"ticket:{ticketId}");
    }
}

