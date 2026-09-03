using System.Collections.Generic;
using System.Linq;
using itehad.Data;
using itehad.Models;

namespace itehad.Helpers
{
    public static class LedgerHelper
    {
        public static Dictionary<int, CustomerBalance> GetAllCustomerBalances(ApplicationDbContext db)
        {
            var result = new Dictionary<int, CustomerBalance>();

            var debits = db.Trips
                .Where(t => t.PaymentMethod == PaymentMethodType.Credit)
                .Select(t => new { t.CustomerId, t.Currency, t.Fare })
                .ToList();

            foreach (var d in debits)
            {
                if (!result.ContainsKey(d.CustomerId)) result[d.CustomerId] = new CustomerBalance();
                if (d.Currency == CurrencyType.ILS) result[d.CustomerId].ILS += d.Fare;
                else result[d.CustomerId].USD += d.Fare;
            }

            var credits = db.CustomerPayments
                .Select(p => new { p.CustomerId, p.Currency, p.Amount })
                .ToList();

            foreach (var c in credits)
            {
                if (!result.ContainsKey(c.CustomerId)) result[c.CustomerId] = new CustomerBalance();
                if (c.Currency == CurrencyType.ILS) result[c.CustomerId].ILS -= c.Amount;
                else result[c.CustomerId].USD -= c.Amount;
            }

            return result;
        }
    }

    public class CustomerBalance
    {
        public decimal ILS { get; set; }
        public decimal USD { get; set; }
    }
}
