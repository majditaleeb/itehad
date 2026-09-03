using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web.Mvc;
using itehad.Data;
using itehad.Helpers;
using itehad.Models;

namespace itehad.Controllers
{
    [Authorize(Roles = Modules.Settings)]
    public class BookingSourcesController : BaseController
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }

        public ActionResult Index()
        {
            var sources = db.BookingSources.OrderBy(b => b.Name).ToList();
            return View(sources);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(BookingSource bookingSource)
        {
            if (!string.IsNullOrWhiteSpace(bookingSource?.Name))
            {
                db.BookingSources.Add(new BookingSource { Name = bookingSource.Name.Trim() });
                db.SaveChanges();
                TempData["Success"] = "تمت إضافة مصدر الحجز بنجاح";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(BookingSource bookingSource)
        {
            var existing = db.BookingSources.Find(bookingSource.Id);
            if (existing != null && !string.IsNullOrWhiteSpace(bookingSource.Name))
            {
                existing.Name = bookingSource.Name.Trim();
                db.SaveChanges();
                TempData["Success"] = "تم تعديل مصدر الحجز بنجاح";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var source = db.BookingSources.Find(id);
            if (source != null)
            {
                db.BookingSources.Remove(source);
                try
                {
                    db.SaveChanges();
                    TempData["Success"] = "تم حذف مصدر الحجز بنجاح";
                }
                catch (DbUpdateException)
                {
                    TempData["Error"] = "لا يمكن حذف هذا المصدر لأنه مستخدم في رحلات سابقة";
                }
            }
            return RedirectToAction("Index");
        }
    }
}
