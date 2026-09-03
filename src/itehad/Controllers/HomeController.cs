using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using itehad.Data;
using itehad.Helpers;
using itehad.Models;
using itehad.Models.ViewModels;

namespace itehad.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }

        public ActionResult Index()
        {
            var today = DateTime.Today;
            var todayTrips = db.Trips.Where(t => DbFunctions.TruncateTime(t.TripDate) == today).ToList();
            var settings = db.AppSettings.Find(1);

            var vm = new DashboardViewModel
            {
                TodayTripsCount = todayTrips.Count,
                TodayCashILS = todayTrips.Where(t => t.PaymentMethod == PaymentMethodType.Cash && t.Currency == CurrencyType.ILS).Sum(t => t.Fare),
                TodayCashUSD = todayTrips.Where(t => t.PaymentMethod == PaymentMethodType.Cash && t.Currency == CurrencyType.USD).Sum(t => t.Fare),
                TodayCreditILS = todayTrips.Where(t => t.PaymentMethod == PaymentMethodType.Credit && t.Currency == CurrencyType.ILS).Sum(t => t.Fare),
                TodayCreditUSD = todayTrips.Where(t => t.PaymentMethod == PaymentMethodType.Credit && t.Currency == CurrencyType.USD).Sum(t => t.Fare),
                DriversOnDutyCount = db.DriverAttendances.Count(a => a.CheckOutTime == null)
            };

            var customersById = db.Customers.ToDictionary(c => c.Id, c => c.Name);
            var balances = LedgerHelper.GetAllCustomerBalances(db);

            var debtors = balances
                .Where(b => b.Value.ILS > 0 || b.Value.USD > 0)
                .Select(b => new CustomerListItemViewModel
                {
                    Id = b.Key,
                    Name = customersById.ContainsKey(b.Key) ? customersById[b.Key] : "",
                    OutstandingILS = b.Value.ILS,
                    OutstandingUSD = b.Value.USD
                })
                .ToList();

            foreach (var d in debtors)
            {
                d.IsOverLimit = (settings.DebtAlertThresholdILS > 0 && d.OutstandingILS > settings.DebtAlertThresholdILS)
                             || (settings.DebtAlertThresholdUSD > 0 && d.OutstandingUSD > settings.DebtAlertThresholdUSD);
            }

            vm.OverLimitCustomersCount = debtors.Count(d => d.IsOverLimit);
            vm.TopDebtors = debtors
                .OrderByDescending(c => c.OutstandingILS + c.OutstandingUSD)
                .Take(5)
                .ToList();

            return View(vm);
        }
    }
}
