using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using itehad.Data;
using itehad.Helpers;
using itehad.Models;
using itehad.Models.ViewModels;

namespace itehad.Controllers
{
    [Authorize(Roles = Modules.Customers)]
    public class CustomersController : BaseController
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }

        public ActionResult Index()
        {
            var customers = db.Customers.OrderBy(c => c.Name).ToList();
            var balances = LedgerHelper.GetAllCustomerBalances(db);
            var settings = db.AppSettings.Find(1);

            var vm = customers.Select(c =>
            {
                var balance = balances.ContainsKey(c.Id) ? balances[c.Id] : new CustomerBalance();
                var item = new CustomerListItemViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Phone = c.Phone,
                    OutstandingILS = balance.ILS,
                    OutstandingUSD = balance.USD
                };
                item.IsOverLimit = (settings.DebtAlertThresholdILS > 0 && item.OutstandingILS > settings.DebtAlertThresholdILS)
                                 || (settings.DebtAlertThresholdUSD > 0 && item.OutstandingUSD > settings.DebtAlertThresholdUSD);
                return item;
            }).ToList();

            return View(vm);
        }

        public ActionResult Statement(int id, DateTime? from, DateTime? to)
        {
            var vm = BuildStatementViewModel(id, from, to);
            if (vm == null) return HttpNotFound();
            return View(vm);
        }

        public ActionResult Print(int id, DateTime? from, DateTime? to)
        {
            var vm = BuildStatementViewModel(id, from, to);
            if (vm == null) return HttpNotFound();
            ViewBag.Title = "كشف حساب - " + vm.Customer.Name;
            return View(vm);
        }

        public ActionResult ExportCsv(int id, DateTime? from, DateTime? to)
        {
            var vm = BuildStatementViewModel(id, from, to);
            if (vm == null) return HttpNotFound();

            var sb = new StringBuilder();

            AppendLedgerCsv(sb, "شيقل", vm.LedgerILS);
            sb.AppendLine();
            AppendLedgerCsv(sb, "دولار", vm.LedgerUSD);

            var bytes = new UTF8Encoding(true).GetBytes(sb.ToString());
            return File(bytes, "text/csv", "كشف-حساب-" + vm.Customer.Name + ".csv");
        }

        private static void AppendLedgerCsv(StringBuilder sb, string currencyLabel, CustomerLedgerViewModel ledger)
        {
            sb.AppendLine(CsvRow("كشف حساب بال" + currencyLabel));
            sb.AppendLine(CsvRow("التاريخ", "البيان", "مدين", "دائن", "الرصيد"));
            sb.AppendLine(CsvRow("", "رصيد افتتاحي", "", "", ledger.OpeningBalance.ToString("0.##")));
            foreach (var e in ledger.Entries)
            {
                sb.AppendLine(CsvRow(e.Date.ToString("yyyy-MM-dd HH:mm"), e.Description,
                    e.Debit > 0 ? e.Debit.ToString("0.##") : "",
                    e.Credit > 0 ? e.Credit.ToString("0.##") : "",
                    e.RunningBalance.ToString("0.##")));
            }
            sb.AppendLine(CsvRow("", "رصيد ختامي", "", "", ledger.ClosingBalance.ToString("0.##")));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddPayment(AddPaymentViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "تعذّر تسجيل الدفعة، تحقق من البيانات المدخلة";
                return RedirectToAction("Statement", new { id = vm.CustomerId });
            }

            db.CustomerPayments.Add(new CustomerPayment
            {
                CustomerId = vm.CustomerId,
                Amount = vm.Amount,
                Currency = vm.Currency,
                PaymentDate = vm.PaymentDate,
                Notes = vm.Notes,
                CreatedAt = DateTime.Now
            });
            db.SaveChanges();

            TempData["Success"] = "تم تسجيل الدفعة بنجاح";
            return RedirectToAction("Statement", new { id = vm.CustomerId });
        }

        private CustomerStatementViewModel BuildStatementViewModel(int id, DateTime? from, DateTime? to)
        {
            var customer = db.Customers.Find(id);
            if (customer == null) return null;

            var toDate = (to ?? DateTime.Today).Date;

            var creditTrips = db.Trips
                .Include(t => t.FromLocation)
                .Include(t => t.ToLocation)
                .Where(t => t.CustomerId == id && t.PaymentMethod == PaymentMethodType.Credit)
                .ToList();

            var payments = db.CustomerPayments
                .Where(p => p.CustomerId == id)
                .ToList();

            var currentOutstandingILS = creditTrips.Where(t => t.Currency == CurrencyType.ILS).Sum(t => t.Fare)
                                       - payments.Where(p => p.Currency == CurrencyType.ILS).Sum(p => p.Amount);
            var currentOutstandingUSD = creditTrips.Where(t => t.Currency == CurrencyType.USD).Sum(t => t.Fare)
                                       - payments.Where(p => p.Currency == CurrencyType.USD).Sum(p => p.Amount);

            return new CustomerStatementViewModel
            {
                Customer = customer,
                FromDate = from,
                ToDate = toDate,
                CurrentOutstandingILS = currentOutstandingILS,
                CurrentOutstandingUSD = currentOutstandingUSD,
                LedgerILS = BuildLedger(creditTrips, payments, CurrencyType.ILS, from, toDate),
                LedgerUSD = BuildLedger(creditTrips, payments, CurrencyType.USD, from, toDate)
            };
        }

        private static CustomerLedgerViewModel BuildLedger(List<Trip> trips, List<CustomerPayment> payments, CurrencyType currency, DateTime? from, DateTime to)
        {
            var debitEntries = trips.Where(t => t.Currency == currency)
                .Select(t => new LedgerEntry
                {
                    Date = t.TripDate,
                    Description = "رحلة: " + t.FromLocation.Name + " ← " + t.ToLocation.Name,
                    Debit = t.Fare,
                    Credit = 0
                });

            var creditEntries = payments.Where(p => p.Currency == currency)
                .Select(p => new LedgerEntry
                {
                    Date = p.PaymentDate,
                    Description = string.IsNullOrWhiteSpace(p.Notes) ? "دفعة" : p.Notes,
                    Debit = 0,
                    Credit = p.Amount
                });

            var allEntries = debitEntries.Concat(creditEntries).OrderBy(e => e.Date).ToList();

            var opening = allEntries
                .Where(e => !from.HasValue || e.Date.Date < from.Value.Date)
                .Sum(e => e.Debit - e.Credit);

            var periodEntries = allEntries
                .Where(e => (!from.HasValue || e.Date.Date >= from.Value.Date) && e.Date.Date <= to)
                .ToList();

            var running = opening;
            foreach (var entry in periodEntries)
            {
                running += entry.Debit - entry.Credit;
                entry.RunningBalance = running;
            }

            return new CustomerLedgerViewModel
            {
                Currency = currency,
                OpeningBalance = opening,
                Entries = periodEntries,
                ClosingBalance = running
            };
        }

        private static string CsvRow(params string[] fields)
        {
            return string.Join(",", fields.Select(f => "\"" + (f ?? "").Replace("\"", "\"\"") + "\""));
        }

        public ActionResult Create()
        {
            return View(new Customer());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Customer customer)
        {
            if (!ModelState.IsValid) return View(customer);

            customer.CreatedDate = DateTime.Now;
            db.Customers.Add(customer);
            db.SaveChanges();

            TempData["Success"] = "تمت إضافة الزبون بنجاح";
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var customer = db.Customers.Find(id);
            if (customer == null) return HttpNotFound();
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Customer customer)
        {
            if (!ModelState.IsValid) return View(customer);

            var existing = db.Customers.Find(customer.Id);
            if (existing == null) return HttpNotFound();

            existing.Name = customer.Name;
            existing.Phone = customer.Phone;
            db.SaveChanges();

            TempData["Success"] = "تم تعديل بيانات الزبون بنجاح";
            return RedirectToAction("Index");
        }
    }
}
