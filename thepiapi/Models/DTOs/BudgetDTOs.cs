namespace thepiapi.Models.DTOs
{
    public class BudgetRequest
    {
        public int CategoryId { get; set; }
        public double Amount { get; set; }
        public string Period { get; set; } = "Monthly"; // Monthly, Weekly
        public DateTime StartDate { get; set; }
    }

    public class BudgetResponse
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryColor { get; set; } = string.Empty;
        public double LimitAmount { get; set; }
        public double SpentAmount { get; set; }
        public double RemainingAmount { get; set; }
        public double ProgressPercentage { get; set; }
    }
}