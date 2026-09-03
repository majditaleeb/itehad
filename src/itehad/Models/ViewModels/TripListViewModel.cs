using System;
using System.Collections.Generic;

namespace itehad.Models.ViewModels
{
    public class TripListViewModel
    {
        public DateTime FilterDate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<Trip> Trips { get; set; }

        /// <summary>بحث الأعمدة المطبَّق على هالقائمة — بينحمل على كل الروابط.</summary>
        public TripFilter Filter { get; set; }

        public bool IsRange { get { return FromDate.Date != ToDate.Date; } }
        public int DaysSpan { get { return (int)(ToDate.Date - FromDate.Date).TotalDays + 1; } }

        public string PeriodLabel
        {
            get
            {
                return IsRange
                    ? FromDate.ToString("yyyy-MM-dd") + " ← " + ToDate.ToString("yyyy-MM-dd")
                    : FromDate.ToString("yyyy-MM-dd");
            }
        }

        // Paging — the table shows one page at a time, the totals and the
        // Excel/PDF exports always cover the whole period.
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }

        public int PageCount
        {
            get
            {
                if (PageSize <= 0) return 1;
                var pages = (TotalCount + PageSize - 1) / PageSize;
                return pages < 1 ? 1 : pages;
            }
        }

        public bool IsPaged { get { return TotalCount > PageSize && PageSize > 0; } }
        public int FirstRowNumber { get { return TotalCount == 0 ? 0 : (Page - 1) * PageSize + 1; } }
        public int LastRowNumber { get { return (Page - 1) * PageSize + (Trips == null ? 0 : Trips.Count); } }

        public decimal CashILS { get; set; }
        public decimal CashUSD { get; set; }
        public decimal CreditTotalILS { get; set; }
        public decimal CreditTotalUSD { get; set; }
    }
}
