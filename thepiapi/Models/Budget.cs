using System;
using System.Collections.Generic;

namespace thepiapi.Models;

public partial class Budget
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int? CategoryId { get; set; }

    public double Amount { get; set; }

    public string Period { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Category? Category { get; set; }

    public virtual User User { get; set; } = null!;
}
