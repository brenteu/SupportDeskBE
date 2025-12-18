using System;
using System.Text.Json.Serialization;
namespace SupportDeskBE.Models
{
    public class Message
    {
        public int Id { get; set; }

        public int TicketId { get; set; }

        [JsonIgnore]
        public Ticket Ticket { get; set; } = null!;

        public string Body { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string AuthorUserId { get; set; } = "";

        public bool IsInternalNote { get; set; } = false;
    }
}

