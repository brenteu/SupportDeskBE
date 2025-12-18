using System.ComponentModel.DataAnnotations;

namespace SupportDeskBE.Dtos
{
    public class CreateMessageDto
    {
        [Required]
        public string Body { get; set; } = "";

        public bool IsInternalNote { get; set; }
    }
}
