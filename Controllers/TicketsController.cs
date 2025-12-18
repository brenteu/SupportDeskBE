using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SupportDeskBE.Auth;
using SupportDeskBE.Data;
using SupportDeskBE.Dtos;
using SupportDeskBE.Hubs;
using SupportDeskBE.Models;
using System;

namespace SupportDeskBE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly SupportDeskDbContext _db;
        private readonly IHubContext<TicketHub> _hub;

        public TicketsController(SupportDeskDbContext db, IHubContext<TicketHub> hub)
        {
            _db = db;
            _hub = hub;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ticket>>> GetAll([FromQuery] TicketStatus? status)
        {
            var userId = AuthHelpers.UserId(User);
            var isStaff = AuthHelpers.IsStaff(User);

            IQueryable<Ticket> q = _db.Tickets.AsNoTracking();

            if (!isStaff)
                q = q.Where(t => t.CreatedByUserId == userId);

            if (status.HasValue)
                q = q.Where(t => t.Status == status.Value);

            var results = await q.OrderByDescending(t => t.UpdatedAt).ToListAsync();
            return Ok(results);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Ticket>> GetById(int id)
        {
            var ticket = await _db.Tickets
                .Include(t => t.Messages)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null) return NotFound();

            var userId = AuthHelpers.UserId(User);
            var isStaff = AuthHelpers.IsStaff(User);

            if (!isStaff && ticket.CreatedByUserId != userId)
                return Forbid();

            // Customers shouldn't see internal notes
            if (!isStaff)
                ticket.Messages = ticket.Messages.Where(m => !m.IsInternalNote).ToList();

            return Ok(ticket);
        }

        [HttpPost]
        public async Task<ActionResult<Ticket>> Create([FromBody] CreateTicketDto dto)
        {
            var userId = AuthHelpers.UserId(User);

            var ticket = new Ticket
            {
                Title = dto.Title.Trim(),
                Description = dto.Description.Trim(),
                Priority = dto.Priority,
                Status = TicketStatus.New,
                CreatedByUserId = userId,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Tickets.Add(ticket);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, ticket);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTicketDto dto)
        {
            var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null) return NotFound();

            var userId = AuthHelpers.UserId(User);
            var isStaff = AuthHelpers.IsStaff(User);

            if (!isStaff && ticket.CreatedByUserId != userId)
                return Forbid();

            // Everyone can update these
            ticket.Status = dto.Status;
            ticket.Priority = dto.Priority;

            // Only staff can assign
            if (isStaff)
                ticket.AssignedToUserId = dto.AssignedToUserId;
            else if (dto.AssignedToUserId != null)
                return Forbid();

            ticket.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            await _hub.Clients
                .Group($"ticket:{ticket.Id}")
                .SendAsync("TicketUpdated", new
                {
                    status = ticket.Status.ToString(),
                    priority = ticket.Priority.ToString()
                });

            return NoContent();

        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null) return NotFound();

            _db.Tickets.Remove(ticket);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}

