using System.ComponentModel.DataAnnotations;

namespace thepiapi.Models.DTOs
{
    public class CreateTransactionRequest
    {
        [Required]
        public int AccountId { get; set; }

        [Required]
        [Range(-double.MaxValue, double.MaxValue)]
        public double Amount { get; set; }

        [Required]
        [MaxLength(255)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        public DateOnly TransactionDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

        [MaxLength(100)]
        public string? Merchant { get; set; }

        [MaxLength(50)]
        public string? PaymentMethod { get; set; }

        public string? Notes { get; set; }
    }

    public class UpdateTransactionRequest
    {
        // Added AccountId so you can move a transaction if you picked the wrong wallet
        [Required]
        public int AccountId { get; set; }

        [Required]
        public double Amount { get; set; }

        [Required]
        [MaxLength(255)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public DateOnly TransactionDate { get; set; }

        [MaxLength(100)]
        public string? Merchant { get; set; }

        public string? Notes { get; set; }
    }

    public class TransactionResponse
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public double Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryColor { get; set; } = string.Empty;
        public string CategoryIcon { get; set; } = string.Empty;
        public DateOnly TransactionDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? Merchant { get; set; }
        public string? PaymentMethod { get; set; }
        public string? Notes { get; set; }
    }

    public class TransactionFilters
    {
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public int? CategoryId { get; set; }
        public int? AccountId { get; set; }
        public string? Search { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    public class TransactionSummaryResponse
    {
        public double TotalIncome { get; set; }
        public double TotalExpenses { get; set; }
        public double NetFlow { get; set; }
        public int TransactionCount { get; set; }
    }

    public class QuickAddTransactionRequest
    {
        [Required]
        public double Amount { get; set; }

        [Required]
        [MaxLength(100)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? CategoryName { get; set; }
    }
}