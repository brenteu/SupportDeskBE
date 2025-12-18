using System;
using System.Collections.Generic;

namespace SupportDeskBE.Models
{
    public enum TicketStatus
    {
        New,
        Open,
        Pending,
        Resolved,
        Closed
    }

    public enum TicketPriority
    {
        Low,
        Medium,
        High,
        Urgent
    }

    public class Ticket
    {
        public int Id { get; set; }

        public string Title { get; set; } = "";
        public string Description { get; set; } = "";

        public TicketStatus Status { get; set; } = TicketStatus.New;
        public TicketPriority Priority { get; set; } = TicketPriority.Medium;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Identity user ids (strings / GUID-like)
        public string CreatedByUserId { get; set; } = "";
        public string? AssignedToUserId { get; set; }

        public List<Message> Messages { get; set; } = new();
    }
}

