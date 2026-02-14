using System.ComponentModel.DataAnnotations;

namespace thepiapi.Models.DTOs
{
    public class BillRequest
    {
        [Required]
        public string Description { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public double Amount { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public string? Frequency { get; set; } // e.g., "Monthly", "Yearly"
    }
}