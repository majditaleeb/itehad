using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web.Mvc;
using itehad.Data;
using itehad.Helpers;
using itehad.Models;

namespace itehad.Controllers
{
    [Authorize(Roles = Modules.Settings)]
    public class LocationsController : BaseController
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }

        public ActionResult Index()
        {
            var locations = db.Locations.OrderBy(l => l.Name).ToList();
            return View(locations);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Location location)
        {
            if (!string.IsNullOrWhiteSpace(location?.Name))
            {
                db.Locations.Add(new Location { Name = location.Name.Trim() });
                db.SaveChanges();
                TempData["Success"] = "تمت إضافة الموقع بنجاح";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Location location)
        {
            var existing = db.Locations.Find(location.Id);
            if (existing != null && !string.IsNullOrWhiteSpace(location.Name))
            {
                existing.Name = location.Name.Trim();
                db.SaveChanges();
                TempData["Success"] = "تم تعديل الموقع بنجاح";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var location = db.Locations.Find(id);
            if (location != null)
            {
                db.Locations.Remove(location);
                try
                {
                    db.SaveChanges();
                    TempData["Success"] = "تم حذف الموقع بنجاح";
                }
                catch (DbUpdateException)
                {
                    TempData["Error"] = "لا يمكن حذف هذا الموقع لأنه مستخدم في رحلات سابقة";
                }
            }
            return RedirectToAction("Index");
        }
    }
}
