using System.Linq;
using System.Web.Mvc;
using itehad.Data;
using itehad.Helpers;
using itehad.Models;

namespace itehad.Controllers
{
    [Authorize(Roles = Modules.Drivers)]
    public class DriversController : BaseController
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }

        public ActionResult Index()
        {
            var drivers = db.Drivers.OrderByDescending(d => d.IsActive).ThenBy(d => d.Name).ToList();
            return View(drivers);
        }

        public ActionResult Create()
        {
            return View(new Driver { IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Driver driver)
        {
            if (!ModelState.IsValid) return View(driver);

            db.Drivers.Add(driver);
            db.SaveChanges();

            TempData["Success"] = "تمت إضافة السائق بنجاح";
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var driver = db.Drivers.Find(id);
            if (driver == null) return HttpNotFound();
            return View(driver);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Driver driver)
        {
            if (!ModelState.IsValid) return View(driver);

            var existing = db.Drivers.Find(driver.Id);
            if (existing == null) return HttpNotFound();

            existing.Name = driver.Name;
            existing.Phone = driver.Phone;
            existing.CarNumber = driver.CarNumber;
            existing.IsActive = driver.IsActive;
            db.SaveChanges();

            TempData["Success"] = "تم تعديل بيانات السائق بنجاح";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleActive(int id)
        {
            var driver = db.Drivers.Find(id);
            if (driver == null) return HttpNotFound();

            driver.IsActive = !driver.IsActive;
            db.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
