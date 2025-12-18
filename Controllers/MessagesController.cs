using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using SupportDeskBE.Hubs;
using SupportDeskBE.Auth;
using SupportDeskBE.Data;
using SupportDeskBE.Dtos;
using SupportDeskBE.Models;
using System;

namespace SupportDeskBE.Controllers
{
    [ApiController]
    [Route("api/tickets/{ticketId:int}/[controller]")]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly SupportDeskDbContext _db;
        private readonly IHubContext<TicketHub> _hub;

        public MessagesController(SupportDeskDbContext db, IHubContext<TicketHub> hub)
        {
            _db = db;
            _hub = hub;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Message>>> GetAll(int ticketId)
        {
            var ticket = await _db.Tickets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == ticketId);
            if (ticket == null) return NotFound();

            var userId = AuthHelpers.UserId(User);
            var isStaff = AuthHelpers.IsStaff(User);

            if (!isStaff && ticket.CreatedByUserId != userId)
                return Forbid();


            IQueryable<Message> q = _db.Messages
            .Where(m => m.TicketId == ticketId);

            if (!isStaff)
                q = q.Where(m => !m.IsInternalNote);

            q = q.OrderBy(m => m.CreatedAt);

            return Ok(await q.ToListAsync());

        }

        [HttpPost]
        public async Task<ActionResult<Message>> Create(int ticketId, [FromBody] CreateMessageDto dto)
        {
            var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);
            if (ticket == null) return NotFound();

            var userId = AuthHelpers.UserId(User);
            var isStaff = AuthHelpers.IsStaff(User);

            if (!isStaff && ticket.CreatedByUserId != userId)
                return Forbid();

            if (!isStaff && dto.IsInternalNote)
                return Forbid();

            if (!isStaff && ticket.Status == TicketStatus.Closed)
                return Conflict(new { message = "Ticket is closed. You can’t send messages anymore." });

            // Only staff can post internal notes
            var internalNote = isStaff && dto.IsInternalNote;

            var msg = new Message
            {
                TicketId = ticketId,
                Body = dto.Body.Trim(),
                AuthorUserId = userId,
                IsInternalNote = internalNote,
                CreatedAt = DateTime.UtcNow
            };

            _db.Messages.Add(msg);

            // helpful workflow: creating a message opens a New ticket
            if (ticket.Status == TicketStatus.New)
                ticket.Status = TicketStatus.Open;

            ticket.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            await _hub.Clients
                .Group($"ticket:{ticketId}")
                .SendAsync("MessageCreated", new
                {
                    id = msg.Id,
                    body = msg.Body,
                    createdAt = msg.CreatedAt,
                    isInternalNote = msg.IsInternalNote
                });

            return Ok(msg);
        }
    }
}
