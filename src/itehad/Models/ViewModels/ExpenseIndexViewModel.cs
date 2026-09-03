using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace itehad.Models.ViewModels
{
    public class ExpenseCategorySummaryRow
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public decimal TotalAmount { get; set; }
        public int Count { get; set; }
    }

    public class DriverExpenseSummaryRow
    {
        public int DriverId { get; set; }
        public string DriverName { get; set; }
        public string CarNumber { get; set; }
        public decimal FuelTotal { get; set; }
        public decimal MaintenanceTotal { get; set; }
        public decimal OtherTotal { get; set; }
        public decimal GrandTotal { get; set; }
    }

    public class ExpenseIndexViewModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int? CategoryId { get; set; }
        public decimal TotalAmount { get; set; }
        public List<ExpenseCategorySummaryRow> Summary { get; set; }
        public List<DriverExpenseSummaryRow> DriverSummary { get; set; }
        public List<Expense> Entries { get; set; }
        public IEnumerable<SelectListItem> CategoryOptions { get; set; }
    }
}
