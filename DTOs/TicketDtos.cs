using System.ComponentModel.DataAnnotations;
using SupportDeskBE.Models;

namespace SupportDeskBE.Dtos
{
    public class CreateTicketDto
    {
        [Required, StringLength(120)]
        public string Title { get; set; } = "";

        [Required, StringLength(4000)]
        public string Description { get; set; } = "";

        [Required]
        public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    }
}


