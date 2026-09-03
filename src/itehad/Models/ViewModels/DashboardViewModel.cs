using System.Collections.Generic;

namespace itehad.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TodayTripsCount { get; set; }
        public decimal TodayCashILS { get; set; }
        public decimal TodayCashUSD { get; set; }
        public decimal TodayCreditILS { get; set; }
        public decimal TodayCreditUSD { get; set; }
        public int DriversOnDutyCount { get; set; }
        public int OverLimitCustomersCount { get; set; }
        public List<CustomerListItemViewModel> TopDebtors { get; set; }
    }
}
