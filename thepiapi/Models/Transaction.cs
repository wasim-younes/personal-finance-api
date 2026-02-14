using System;
using System.Collections.Generic;

namespace thepiapi.Models;

public partial class Transaction
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int AccountId { get; set; }

    public double Amount { get; set; }

    public string Description { get; set; } = null!;

    public int CategoryId { get; set; }

    public DateOnly TransactionDate { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? Merchant { get; set; }

    public string? PaymentMethod { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Category Category { get; set; } = null!;

    public virtual User User { get; set; } = null!;
    public string? Notes { get; set; }
}
