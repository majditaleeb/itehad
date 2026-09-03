using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using itehad.Data;
using itehad.Helpers;
using itehad.Models;
using itehad.Models.ViewModels;

namespace itehad.Controllers
{
    public class ReportsController : BaseController
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }

        [Authorize(Roles = Modules.HoursReport)]
        public ActionResult Hours(DateTime? from, DateTime? to)
        {
            var fromDate = (from ?? DateTime.Today).Date;
            var toDate = (to ?? DateTime.Today).Date;

            var records = db.DriverAttendances
                .Include(a => a.Driver)
                .Where(a => DbFunctions.TruncateTime(a.CheckInTime) >= fromDate
                            && DbFunctions.TruncateTime(a.CheckInTime) <= toDate
                            && a.CheckOutTime != null)
                .ToList();

            var openDriverIds = db.DriverAttendances.Where(a => a.CheckOutTime == null).Select(a => a.DriverId).ToList();

            var rows = records
                .GroupBy(a => new { a.DriverId, a.Driver.Name })
                .Select(g => new DriverHoursReportRow
                {
                    DriverId = g.Key.DriverId,
                    DriverName = g.Key.Name,
                    TotalHours = g.Sum(a => (a.CheckOutTime.Value - a.CheckInTime).TotalHours),
                    IsCurrentlyOnDuty = openDriverIds.Contains(g.Key.DriverId)
                })
                .OrderByDescending(r => r.TotalHours)
                .ToList();

            foreach (var driverId in openDriverIds.Except(rows.Select(r => r.DriverId)))
            {
                var driver = db.Drivers.Find(driverId);
                if (driver != null)
                {
                    rows.Add(new DriverHoursReportRow { DriverId = driverId, DriverName = driver.Name, TotalHours = 0, IsCurrentlyOnDuty = true });
                }
            }

            var absences = BuildAbsenceRows(fromDate, toDate);

            var vm = new HoursReportViewModel { FromDate = fromDate, ToDate = toDate, Rows = rows, Absences = absences };
            return View(vm);
        }

        private List<AbsenceRow> BuildAbsenceRows(DateTime fromDate, DateTime toDate)
        {
            var activeDrivers = db.Drivers.Where(d => d.IsActive).ToList();

            var attendanceInRange = db.DriverAttendances
                .Where(a => DbFunctions.TruncateTime(a.CheckInTime) >= fromDate && DbFunctions.TruncateTime(a.CheckInTime) <= toDate)
                .Select(a => new { a.DriverId, a.CheckInTime })
                .ToList();

            var totalDays = (toDate - fromDate).Days + 1;
            var allDates = Enumerable.Range(0, totalDays).Select(i => fromDate.AddDays(i)).ToList();

            return activeDrivers
                .Select(d =>
                {
                    var presentDates = attendanceInRange
                        .Where(a => a.DriverId == d.Id)
                        .Select(a => a.CheckInTime.Date)
                        .Distinct()
                        .ToHashSet();

                    var missedDates = allDates.Where(dt => !presentDates.Contains(dt)).ToList();
                    return new AbsenceRow { DriverName = d.Name, AbsentDates = missedDates };
                })
                .Where(r => r.AbsentDates.Any())
                .OrderByDescending(r => r.AbsentDates.Count)
                .ToList();
        }

        [Authorize(Roles = Modules.ProfitReport)]
        public ActionResult Profit(DateTime? from, DateTime? to)
        {
            var fromDate = (from ?? DateTime.Today.AddDays(-30)).Date;
            var toDate = (to ?? DateTime.Today).Date;

            var trips = db.Trips
                .Include(t => t.BookingSource)
                .Where(t => DbFunctions.TruncateTime(t.TripDate) >= fromDate && DbFunctions.TruncateTime(t.TripDate) <= toDate)
                .ToList();

            var rows = trips
                .GroupBy(t => new { t.BookingSourceId, t.BookingSource.Name })
                .Select(g => new ProfitReportRow
                {
                    BookingSourceName = g.Key.Name,
                    TotalILS = g.Where(t => t.Currency == CurrencyType.ILS).Sum(t => t.Fare),
                    TotalUSD = g.Where(t => t.Currency == CurrencyType.USD).Sum(t => t.Fare),
                    TripCount = g.Count()
                })
                .OrderByDescending(r => r.TotalILS + r.TotalUSD)
                .ToList();

            var expenses = db.Expenses
                .Include(e => e.Category)
                .Where(e => DbFunctions.TruncateTime(e.InvoiceDate) >= fromDate && DbFunctions.TruncateTime(e.InvoiceDate) <= toDate)
                .ToList();

            var expensesByCategory = expenses
                .GroupBy(e => new { e.CategoryId, e.Category.Name })
                .Select(g => new ExpenseCategorySummaryRow
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.Name,
                    TotalAmount = g.Sum(e => e.Amount),
                    Count = g.Count()
                })
                .OrderByDescending(r => r.TotalAmount)
                .ToList();

            var totalExpenses = expenses.Sum(e => e.Amount);
            var totalRevenueIls = rows.Sum(r => r.TotalILS);

            var vm = new ProfitReportViewModel
            {
                FromDate = fromDate,
                ToDate = toDate,
                Rows = rows,
                TotalRevenueILS = totalRevenueIls,
                TotalRevenueUSD = rows.Sum(r => r.TotalUSD),
                TotalExpenses = totalExpenses,
                ExpensesByCategory = expensesByCategory,
                NetProfitILS = totalRevenueIls - totalExpenses
            };
            return View(vm);
        }
    }
}
