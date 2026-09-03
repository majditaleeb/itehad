using System;
using System.Collections.Generic;

namespace itehad.Models.ViewModels
{
    public class ProfitReportRow
    {
        public string BookingSourceName { get; set; }
        public decimal TotalILS { get; set; }
        public decimal TotalUSD { get; set; }
        public int TripCount { get; set; }
    }

    public class ProfitReportViewModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<ProfitReportRow> Rows { get; set; }
        public decimal TotalRevenueILS { get; set; }
        public decimal TotalRevenueUSD { get; set; }
        public decimal TotalExpenses { get; set; }
        public List<ExpenseCategorySummaryRow> ExpensesByCategory { get; set; }
        public decimal NetProfitILS { get; set; }
    }
}
