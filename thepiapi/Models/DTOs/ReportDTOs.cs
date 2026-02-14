namespace thepiapi.Models.DTOs
{
    public class MonthlyTrend
    {
        public string Month { get; set; } = string.Empty;
        public double Income { get; set; }
        public double Expenses { get; set; }
        public double Savings => Income - Expenses;
    }

    public class CategoryReport
    {
        public string CategoryName { get; set; } = string.Empty;
        public double Amount { get; set; }
        public string Color { get; set; } = string.Empty;
    }
}