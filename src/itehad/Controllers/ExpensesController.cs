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
    [Authorize(Roles = Modules.Expenses)]
    public class ExpensesController : BaseController
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }

        public ActionResult Index(DateTime? from, DateTime? to, int? categoryId)
        {
            var fromDate = (from ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).Date;
            var toDate = (to ?? DateTime.Today).Date;

            var query = db.Expenses
                .Include(e => e.Category)
                .Include(e => e.Driver)
                .Where(e => DbFunctions.TruncateTime(e.InvoiceDate) >= fromDate && DbFunctions.TruncateTime(e.InvoiceDate) <= toDate);

            if (categoryId.HasValue)
            {
                query = query.Where(e => e.CategoryId == categoryId.Value);
            }

            var entries = query.OrderByDescending(e => e.InvoiceDate).ToList();

            var summary = entries
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

            var driverSummary = entries
                .Where(e => e.DriverId.HasValue)
                .GroupBy(e => new { e.DriverId, e.Driver.Name, e.Driver.CarNumber })
                .Select(g => new DriverExpenseSummaryRow
                {
                    DriverId = g.Key.DriverId.Value,
                    DriverName = g.Key.Name,
                    CarNumber = g.Key.CarNumber,
                    FuelTotal = g.Where(e => e.Category.Name == "سولار").Sum(e => e.Amount),
                    MaintenanceTotal = g.Where(e => e.Category.Name == "صيانة").Sum(e => e.Amount),
                    OtherTotal = g.Where(e => e.Category.Name != "سولار" && e.Category.Name != "صيانة").Sum(e => e.Amount),
                    GrandTotal = g.Sum(e => e.Amount)
                })
                .OrderByDescending(r => r.GrandTotal)
                .ToList();

            var vm = new ExpenseIndexViewModel
            {
                FromDate = fromDate,
                ToDate = toDate,
                CategoryId = categoryId,
                TotalAmount = entries.Sum(e => e.Amount),
                Summary = summary,
                DriverSummary = driverSummary,
                Entries = entries,
                CategoryOptions = db.ExpenseCategories.OrderBy(c => c.Name)
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList()
            };

            return View(vm);
        }

        public ActionResult Create()
        {
            var vm = new ExpenseFormViewModel { InvoiceDate = DateTime.Now };
            PopulateCategories(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CreateCategoryAjax(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Json(new { success = false, message = "اسم التصنيف مطلوب" });

            var trimmed = name.Trim();
            var existing = db.ExpenseCategories.FirstOrDefault(c => c.Name == trimmed);
            if (existing != null)
                return Json(new { success = true, id = existing.Id, name = existing.Name });

            var category = new ExpenseCategory { Name = trimmed };
            db.ExpenseCategories.Add(category);
            db.SaveChanges();

            return Json(new { success = true, id = category.Id, name = category.Name });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ExpenseFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                PopulateCategories(vm);
                return View(vm);
            }

            db.Expenses.Add(new Expense
            {
                CategoryId = vm.CategoryId,
                DriverId = vm.DriverId,
                InvoiceNumber = vm.InvoiceNumber,
                VendorName = vm.VendorName,
                VendorLicenseNumber = vm.VendorLicenseNumber,
                InvoiceDate = vm.InvoiceDate,
                Amount = vm.Amount,
                Notes = vm.Notes,
                CreatedAt = DateTime.Now
            });
            db.SaveChanges();

            TempData["Success"] = "تم تسجيل المصروف بنجاح";
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var entry = db.Expenses.Find(id);
            if (entry == null) return HttpNotFound();

            var vm = new ExpenseFormViewModel
            {
                Id = entry.Id,
                CategoryId = entry.CategoryId,
                DriverId = entry.DriverId,
                InvoiceNumber = entry.InvoiceNumber,
                VendorName = entry.VendorName,
                VendorLicenseNumber = entry.VendorLicenseNumber,
                InvoiceDate = entry.InvoiceDate,
                Amount = entry.Amount,
                Notes = entry.Notes
            };
            PopulateCategories(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ExpenseFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                PopulateCategories(vm);
                return View(vm);
            }

            var entry = db.Expenses.Find(vm.Id);
            if (entry == null) return HttpNotFound();

            entry.CategoryId = vm.CategoryId;
            entry.DriverId = vm.DriverId;
            entry.InvoiceNumber = vm.InvoiceNumber;
            entry.VendorName = vm.VendorName;
            entry.VendorLicenseNumber = vm.VendorLicenseNumber;
            entry.InvoiceDate = vm.InvoiceDate;
            entry.Amount = vm.Amount;
            entry.Notes = vm.Notes;
            db.SaveChanges();

            TempData["Success"] = "تم تعديل المصروف بنجاح";
            return RedirectToAction("Index");
        }

        private void PopulateCategories(ExpenseFormViewModel vm)
        {
            vm.CategoryOptions = db.ExpenseCategories.OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList();

            vm.DriverOptions = db.Drivers.Where(d => d.IsActive).OrderBy(d => d.Name)
                .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name + " - " + d.CarNumber }).ToList();
        }
    }
}
