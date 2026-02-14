using System;
using System.Collections.Generic;

namespace thepiapi.Models;

public partial class Bill
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Name { get; set; } = null!;

    public double Amount { get; set; }

    public int CategoryId { get; set; }

    public int DueDay { get; set; }

    public string Frequency { get; set; } = null!;

    public DateTime NextDueDate { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
