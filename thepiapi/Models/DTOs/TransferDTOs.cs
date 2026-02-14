namespace thepiapi.Models.DTOs
{
    public class TransferRequest
    {
        public int FromAccountId { get; set; }
        public int ToAccountId { get; set; }
        public double Amount { get; set; }
        public string? Notes { get; set; }
        public DateTime TransferDate { get; set; } = DateTime.UtcNow;
    }
}