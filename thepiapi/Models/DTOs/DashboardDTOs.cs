using System.ComponentModel.DataAnnotations;

namespace thepiapi.Models.DTOs
{
    public class DashboardResponse
    {
        public FinancialSummary Summary { get; set; } = null!;
        public List<AccountBalance> Accounts { get; set; } = new List<AccountBalance>();
        public List<BudgetProgress> Budgets { get; set; } = new List<BudgetProgress>();
        public List<TransactionResponse> RecentTransactions { get; set; } = new List<TransactionResponse>();
        public List<UpcomingBill> UpcomingBills { get; set; } = new List<UpcomingBill>();
        public List<CategorySpending> CategorySpending { get; set; } = new List<CategorySpending>();
    }

    public class FinancialSummary
    {
        public double TotalBalance { get; set; } // Cash + cards total
        public double MonthlyIncome { get; set; }
        public double MonthlyExpenses { get; set; }
        public double MonthlyNet { get; set; } // Income - Expenses
        public double TodaysSpending { get; set; }
        public double ThisWeekSpending { get; set; }
        public int ActiveBudgets { get; set; }
        public int UpcomingBillsCount { get; set; }
    }

    public class AccountBalance
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public double Balance { get; set; }
        public string Color { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public double ThisMonthChange { get; set; } // How much spent/added this month
    }

    public class BudgetProgress
    {
        public int Id { get; set; }
        public string? CategoryName { get; set; } // Null for "Total Budget"
        public double BudgetAmount { get; set; }
        public double Spent { get; set; }
        public double Remaining { get; set; }
        public double Percentage { get; set; } // 0-100
        public string Status { get; set; } = "On Track"; // "On Track", "Almost Reached", "Exceeded"
        public string Color { get; set; } = string.Empty;
    }

    public class UpcomingBill
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Amount { get; set; }
        public DateTime DueDate { get; set; }
        public int DaysUntilDue { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public bool IsOverdue { get; set; }
    }

    public class CategorySpending
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryColor { get; set; } = string.Empty;
        public string CategoryIcon { get; set; } = string.Empty;
        public double Amount { get; set; }
        public double Percentage { get; set; } // Of total expenses
        public int TransactionCount { get; set; }
    }
}