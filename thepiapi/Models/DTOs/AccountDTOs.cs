namespace thepiapi.Models.DTOs
{
    public class CreateAccountRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // e.g., "Cash", "Bank", "Credit Card"
        public double Balance { get; set; }
        public string Currency { get; set; } = "USD";
        public string? Color { get; set; }
        public string? Icon { get; set; }
    }

    public class AccountResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public double Balance { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string? Color { get; set; }
        public string? Icon { get; set; }
    }
    public class ReconcileRequest
    {
        public double ActualBalance { get; set; }
    }
}