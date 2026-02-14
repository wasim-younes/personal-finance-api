using System;
using System.Collections.Generic;

namespace thepiapi.Models;

public partial class SavingsGoal
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Name { get; set; } = null!;

    public double TargetAmount { get; set; }

    public double? CurrentAmount { get; set; }

    public DateTime? TargetDate { get; set; }

    public bool? IsCompleted { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
