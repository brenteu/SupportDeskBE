using System.ComponentModel.DataAnnotations;
using SupportDeskBE.Models;

namespace SupportDeskBE.Dtos
{
    public class UpdateTicketDto
    {
        [Required]
        public TicketStatus Status { get; set; }

        public string? AssignedToUserId { get; set; }

        [Required]
        public TicketPriority Priority { get; set; }
    }
}

