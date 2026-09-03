using System;
using System.Collections.Generic;

namespace itehad.Models.ViewModels
{
    public class LedgerEntry
    {
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal RunningBalance { get; set; }
    }

    public class CustomerLedgerViewModel
    {
        public CurrencyType Currency { get; set; }
        public decimal OpeningBalance { get; set; }
        public List<LedgerEntry> Entries { get; set; }
        public decimal ClosingBalance { get; set; }
    }

    public class CustomerStatementViewModel
    {
        public Customer Customer { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public decimal CurrentOutstandingILS { get; set; }
        public decimal CurrentOutstandingUSD { get; set; }

        public CustomerLedgerViewModel LedgerILS { get; set; }
        public CustomerLedgerViewModel LedgerUSD { get; set; }
    }
}
