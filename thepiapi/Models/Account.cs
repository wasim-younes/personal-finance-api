using System;
using System.Collections.Generic;

namespace thepiapi.Models;

public partial class Account
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Name { get; set; } = null!;

    public string Type { get; set; } = null!;

    public double? Balance { get; set; }

    public string? Currency { get; set; }

    public string? AccountNumber { get; set; }

    public string? Institution { get; set; }

    public string? Color { get; set; }

    public string? Icon { get; set; }

    public bool? IsActive { get; set; }

    public bool? IncludeInTotal { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual User User { get; set; } = null!;
}
