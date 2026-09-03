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
    [Authorize(Roles = Modules.Trips)]
    public class TripsController : BaseController
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }

        private const int DefaultPageSize = 100;

        public ActionResult Index(DateTime? date, DateTime? from, DateTime? to, int? page, int? pageSize,
                                  TripFilter filter)
        {
            var size = pageSize.HasValue && pageSize.Value > 0 ? Math.Min(pageSize.Value, 1000) : DefaultPageSize;
            var vm = BuildTripListViewModel(date, from, to, page ?? 1, size, filter);
            return View(vm);
        }

        /// <summary>
        /// نفس محتوى القائمة بس كقطعة HTML — بيستدعيها البحث الحيّ فوق الأعمدة
        /// عشان يبدّل الكروت والصفوف والترقيم بدون ما يعيد تحميل الصفحة (ولا
        /// يضيّع اللي المستخدم عم يكتبه بخانات البحث).
        /// </summary>
        public ActionResult List(DateTime? date, DateTime? from, DateTime? to, int? page, int? pageSize,
                                 TripFilter filter)
        {
            var size = pageSize.HasValue && pageSize.Value > 0 ? Math.Min(pageSize.Value, 1000) : DefaultPageSize;
            var vm = BuildTripListViewModel(date, from, to, page ?? 1, size, filter);
            return PartialView("_TripListBody", vm);
        }

        public ActionResult Print(DateTime? date, DateTime? from, DateTime? to, TripFilter filter)
        {
            var vm = BuildTripListViewModel(date, from, to, 1, 0, filter);
            ViewBag.Title = "سجل الحركة اليومية - " + vm.PeriodLabel;
            return View(vm);
        }

        public ActionResult ExportCsv(DateTime? date, DateTime? from, DateTime? to, TripFilter filter)
        {
            var vm = BuildTripListViewModel(date, from, to, 1, 0, filter);

            var sb = new StringBuilder();
            sb.AppendLine("التاريخ,الوقت,المصدر,الزبون,من,إلى,السائقون,الأجرة,العملة,طريقة الدفع,ملاحظات");
            foreach (var t in vm.Trips)
            {
                var drivers = string.Join(" | ", t.TripDrivers.Select(td => td.Driver.Name));
                var currency = t.Currency == CurrencyType.ILS ? "شيقل" : "دولار";
                var payment = t.PaymentMethod == PaymentMethodType.Cash ? "نقدي" : "ذمم";
                sb.AppendLine(CsvRow(t.TripDate.ToString("yyyy-MM-dd"), t.TripDate.ToString("HH:mm"),
                    t.BookingSource.Name, t.Customer.Name,
                    t.FromLocation.Name, t.ToLocation.Name, drivers, t.Fare.ToString("0.##"), currency, payment, t.Notes));
            }

            var bytes = new UTF8Encoding(true).GetBytes(sb.ToString());
            return File(bytes, "text/csv", "سجل-الحركة-" + vm.PeriodLabel.Replace(" ← ", "_") + ".csv");
        }

        /// <summary>
        /// The trips behind one of the four summary cards on the listing — the same
        /// period, narrowed to a single payment method + currency. The card totals are
        /// SQL aggregates over the whole period, so this breakdown ignores paging too;
        /// only the rendered rows are capped, never the reported total.
        /// </summary>
        private const int BreakdownRowCap = 500;

        public ActionResult Breakdown(DateTime? date, DateTime? from, DateTime? to,
                                      int payment, int currency, TripFilter filter)
        {
            filter = filter ?? new TripFilter();

            DateTime start, end;
            ResolvePeriod(date, from, to, out start, out end);
            var endExclusive = end.AddDays(1);

            var pm = payment == 1 ? PaymentMethodType.Credit : PaymentMethodType.Cash;
            var cur = currency == 1 ? CurrencyType.USD : CurrencyType.ILS;

            db.Database.CommandTimeout = 300;
            db.Configuration.ProxyCreationEnabled = false;
            db.Configuration.LazyLoadingEnabled = false;
            db.Configuration.AutoDetectChangesEnabled = false;

            var matching = filter.Apply(db.Trips.Where(t => t.TripDate >= start && t.TripDate < endExclusive
                                                            && t.PaymentMethod == pm && t.Currency == cur));

            var totalCount = matching.Count();
            var total = totalCount == 0 ? 0m : matching.Sum(t => t.Fare);

            var trips = matching
                .AsNoTracking()
                .Include(t => t.BookingSource)
                .Include(t => t.Customer)
                .Include(t => t.FromLocation)
                .Include(t => t.ToLocation)
                .OrderByDescending(t => t.TripDate)
                .ThenByDescending(t => t.Id)
                .Take(BreakdownRowCap)
                .ToList();

            // Second pass for the drivers: joining them above would multiply the rows
            // and break the Take() above.
            if (trips.Count > 0)
            {
                var ids = trips.Select(t => t.Id).ToList();
                var links = db.TripDrivers
                    .AsNoTracking()
                    .Include(td => td.Driver)
                    .Where(td => ids.Contains(td.TripId))
                    .ToList();

                var byTrip = links.GroupBy(l => l.TripId)
                                  .ToDictionary(g => g.Key, g => (ICollection<TripDriver>)g.ToList());
                foreach (var t in trips)
                {
                    ICollection<TripDriver> mine;
                    t.TripDrivers = byTrip.TryGetValue(t.Id, out mine) ? mine : new List<TripDriver>();
                }
            }

            ViewBag.Total = total;
            ViewBag.Symbol = cur == CurrencyType.ILS ? "₪" : "$";
            ViewBag.RowCap = BreakdownRowCap;

            return PartialView("_Breakdown", new TripListViewModel
            {
                FilterDate = start,
                FromDate = start,
                ToDate = end,
                Trips = trips,
                Page = 1,
                PageSize = 0,
                TotalCount = totalCount
            });
        }

        /// <summary>
        /// Resolves the requested period: an explicit from/to range when either is
        /// supplied, otherwise the single <paramref name="date"/> day (today by default).
        /// </summary>
        private static void ResolvePeriod(DateTime? date, DateTime? from, DateTime? to,
                                          out DateTime start, out DateTime end)
        {
            if (from.HasValue || to.HasValue)
            {
                start = (from ?? to.Value).Date;
                end = (to ?? from.Value).Date;
                if (end < start)
                {
                    var swap = start;
                    start = end;
                    end = swap;
                }
            }
            else
            {
                start = end = (date ?? DateTime.Today).Date;
            }
        }

        /// <summary>
        /// Loads one period. <paramref name="pageSize"/> = 0 means "no paging" and is
        /// what Print/ExportCsv use; the totals are always aggregated in SQL over the
        /// whole period, never over the loaded page.
        /// </summary>
        private TripListViewModel BuildTripListViewModel(DateTime? date, DateTime? from, DateTime? to,
                                                         int page, int pageSize, TripFilter filter)
        {
            filter = filter ?? new TripFilter();

            DateTime start, end;
            ResolvePeriod(date, from, to, out start, out end);
            var endExclusive = end.AddDays(1);

            // A wide range can cover thousands of trips, so give the exports room.
            db.Database.CommandTimeout = 300;

            // Read-only listing: no proxies means no accidental lazy-load round trip
            // per row, and materialising a few thousand trips gets much cheaper.
            db.Configuration.ProxyCreationEnabled = false;
            db.Configuration.LazyLoadingEnabled = false;
            db.Configuration.AutoDetectChangesEnabled = false;

            // بحث الأعمدة بينطبق قبل العدّ وقبل الجمع، فالكروت والعدّاد بيطابقوا
            // اللي ظاهر بالجدول بالضبط.
            var period = filter.Apply(db.Trips.Where(t => t.TripDate >= start && t.TripDate < endExclusive));
            var totalCount = period.Count();

            if (pageSize > 0)
            {
                var pages = Math.Max(1, (totalCount + pageSize - 1) / pageSize);
                page = page < 1 ? 1 : (page > pages ? pages : page);
            }
            else
            {
                page = 1;
            }

            var ordered = period
                .AsNoTracking()
                .Include(t => t.BookingSource)
                .Include(t => t.Customer)
                .Include(t => t.FromLocation)
                .Include(t => t.ToLocation)
                .OrderByDescending(t => t.TripDate)
                .ThenByDescending(t => t.Id);

            List<Trip> trips;
            if (pageSize > 0)
            {
                // One page is small enough to pull the drivers in the same query.
                trips = ordered
                    .Include(t => t.TripDrivers.Select(td => td.Driver))
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
            }
            else
            {
                // Whole period (print / CSV): including the driver collection here would
                // multiply every trip row, so the links are fetched in a second pass and
                // the context wires them onto the trips.
                trips = ordered.ToList();
                if (trips.Count > 0)
                {
                    var links = db.TripDrivers
                        .AsNoTracking()
                        .Include(td => td.Driver)
                        .Where(td => td.Trip.TripDate >= start && td.Trip.TripDate < endExclusive)
                        .ToList();

                    var byTrip = links.GroupBy(l => l.TripId)
                                      .ToDictionary(g => g.Key, g => (ICollection<TripDriver>)g.ToList());
                    foreach (var t in trips)
                    {
                        ICollection<TripDriver> mine;
                        t.TripDrivers = byTrip.TryGetValue(t.Id, out mine) ? mine : new List<TripDriver>();
                    }
                }
            }

            var sums = period
                .GroupBy(t => new { t.PaymentMethod, t.Currency })
                .Select(g => new { g.Key.PaymentMethod, g.Key.Currency, Total = g.Sum(t => t.Fare) })
                .ToList();

            Func<PaymentMethodType, CurrencyType, decimal> sum = (pm, cur) =>
            {
                var hit = sums.FirstOrDefault(s => s.PaymentMethod == pm && s.Currency == cur);
                return hit == null ? 0m : hit.Total;
            };

            return new TripListViewModel
            {
                FilterDate = start,
                FromDate = start,
                ToDate = end,
                Trips = trips,
                Filter = filter,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                CashILS = sum(PaymentMethodType.Cash, CurrencyType.ILS),
                CashUSD = sum(PaymentMethodType.Cash, CurrencyType.USD),
                CreditTotalILS = sum(PaymentMethodType.Credit, CurrencyType.ILS),
                CreditTotalUSD = sum(PaymentMethodType.Credit, CurrencyType.USD)
            };
        }

        private static string CsvRow(params string[] fields)
        {
            return string.Join(",", fields.Select(f => "\"" + (f ?? "").Replace("\"", "\"\"") + "\""));
        }

        public ActionResult Create()
        {
            var vm = new TripFormViewModel
            {
                TripDate = DateTime.Now,
                RequestType = TripRequestType.Transfer,
                Currency = CurrencyType.ILS,
                PaymentMethod = PaymentMethodType.Cash
            };
            PopulateDropdowns(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TripFormViewModel vm)
        {
            ValidateTripForm(vm);

            if (!ModelState.IsValid)
            {
                PopulateDropdowns(vm);
                return View(vm);
            }

            var trip = new Trip
            {
                TripDate = vm.TripDate,
                BookingSourceId = vm.BookingSourceId,
                CustomerId = vm.CustomerId,
                RequestType = vm.RequestType,
                DaysCount = vm.RequestType == TripRequestType.MultiDay ? vm.DaysCount : null,
                FromLocationId = vm.FromLocationId,
                ToLocationId = vm.ToLocationId,
                Fare = vm.Fare,
                Currency = vm.Currency,
                PaymentMethod = vm.PaymentMethod,
                IsSettled = vm.PaymentMethod == PaymentMethodType.Cash,
                Notes = vm.Notes,
                CreatedAt = DateTime.Now
            };

            foreach (var driverId in vm.DriverIds.Distinct())
            {
                trip.TripDrivers.Add(new TripDriver { DriverId = driverId });
            }

            db.Trips.Add(trip);
            db.SaveChanges();

            TempData["Success"] = "تم تسجيل الرحلة بنجاح";
            return RedirectToAction("Index", new { date = vm.TripDate.Date.ToString("yyyy-MM-dd") });
        }

        public ActionResult Edit(int id)
        {
            var trip = db.Trips.Include(t => t.TripDrivers).FirstOrDefault(t => t.Id == id);
            if (trip == null) return HttpNotFound();

            var vm = new TripFormViewModel
            {
                Id = trip.Id,
                TripDate = trip.TripDate,
                BookingSourceId = trip.BookingSourceId,
                CustomerId = trip.CustomerId,
                RequestType = trip.RequestType,
                DaysCount = trip.DaysCount,
                FromLocationId = trip.FromLocationId,
                ToLocationId = trip.ToLocationId,
                DriverIds = trip.TripDrivers.Select(td => td.DriverId).ToArray(),
                Fare = trip.Fare,
                Currency = trip.Currency,
                PaymentMethod = trip.PaymentMethod,
                Notes = trip.Notes
            };
            PopulateDropdowns(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(TripFormViewModel vm)
        {
            ValidateTripForm(vm);

            if (!ModelState.IsValid)
            {
                PopulateDropdowns(vm);
                return View(vm);
            }

            var trip = db.Trips.Include(t => t.TripDrivers).FirstOrDefault(t => t.Id == vm.Id);
            if (trip == null) return HttpNotFound();

            trip.TripDate = vm.TripDate;
            trip.BookingSourceId = vm.BookingSourceId;
            trip.CustomerId = vm.CustomerId;
            trip.RequestType = vm.RequestType;
            trip.DaysCount = vm.RequestType == TripRequestType.MultiDay ? vm.DaysCount : null;
            trip.FromLocationId = vm.FromLocationId;
            trip.ToLocationId = vm.ToLocationId;
            trip.Fare = vm.Fare;
            trip.Currency = vm.Currency;
            trip.PaymentMethod = vm.PaymentMethod;
            trip.Notes = vm.Notes;
            if (vm.PaymentMethod == PaymentMethodType.Cash)
            {
                trip.IsSettled = true;
            }

            var newDriverIds = vm.DriverIds.Distinct().ToList();
            var toRemove = trip.TripDrivers.Where(td => !newDriverIds.Contains(td.DriverId)).ToList();
            foreach (var td in toRemove) trip.TripDrivers.Remove(td);
            var existingDriverIds = trip.TripDrivers.Select(td => td.DriverId).ToList();
            foreach (var driverId in newDriverIds.Except(existingDriverIds))
            {
                trip.TripDrivers.Add(new TripDriver { TripId = trip.Id, DriverId = driverId });
            }

            db.SaveChanges();

            TempData["Success"] = "تم تعديل الرحلة بنجاح";
            return RedirectToAction("Index", new { date = trip.TripDate.Date.ToString("yyyy-MM-dd") });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CreateCustomerAjax(string name, string phone)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Json(new { success = false, message = "اسم الزبون مطلوب" });

            var customer = new Customer { Name = name.Trim(), Phone = phone, CreatedDate = DateTime.Now };
            db.Customers.Add(customer);
            db.SaveChanges();

            return Json(new { success = true, id = customer.Id, name = customer.Name });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CreateLocationAjax(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Json(new { success = false, message = "اسم الموقع مطلوب" });

            var location = new Location { Name = name.Trim() };
            db.Locations.Add(location);
            db.SaveChanges();

            return Json(new { success = true, id = location.Id, name = location.Name });
        }

        private void ValidateTripForm(TripFormViewModel vm)
        {
            if (vm.RequestType == TripRequestType.MultiDay && (vm.DaysCount == null || vm.DaysCount <= 0))
            {
                ModelState.AddModelError("DaysCount", "عدد الأيام مطلوب عند اختيار حجز لعدة أيام");
            }

            if (vm.DriverIds == null || vm.DriverIds.Length == 0)
            {
                ModelState.AddModelError("DriverIds", "يجب اختيار سائق واحد على الأقل");
            }

            if (vm.FromLocationId == vm.ToLocationId && vm.FromLocationId != 0)
            {
                ModelState.AddModelError("ToLocationId", "موقع الوجهة يجب أن يختلف عن موقع الانطلاق");
            }
        }

        private void PopulateDropdowns(TripFormViewModel vm)
        {
            vm.BookingSourceOptions = db.BookingSources.OrderBy(b => b.Name)
                .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Name }).ToList();

            vm.CustomerOptions = db.Customers.OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList();

            // المواقع والسائقين القدام (المستوردين) مخفيين، بس بنضل نعرض اللي
            // مختارين فعلاً على الرحلة حتى تعديل رحلة قديمة ما يضيّع بياناتها.
            var fromId = vm.FromLocationId;
            var toId = vm.ToLocationId;
            vm.LocationOptions = db.Locations
                .Where(l => l.IsActive || l.Id == fromId || l.Id == toId)
                .OrderBy(l => l.Name)
                .Select(l => new SelectListItem { Value = l.Id.ToString(), Text = l.Name }).ToList();

            var pickedDrivers = vm.DriverIds ?? new int[0];
            vm.DriverOptions = db.Drivers
                .Where(d => d.IsActive || pickedDrivers.Contains(d.Id))
                .OrderBy(d => d.Name)
                .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name + " - " + d.CarNumber }).ToList();
        }
    }
}
