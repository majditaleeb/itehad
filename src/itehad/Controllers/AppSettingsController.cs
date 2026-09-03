using System.Linq;
using System.Web.Mvc;
using itehad.Data;
using itehad.Helpers;
using itehad.Models;

namespace itehad.Controllers
{
    [Authorize(Roles = Modules.Settings)]
    public class AppSettingsController : BaseController
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }

        public ActionResult Index()
        {
            var settings = db.AppSettings.Find(1);
            return View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(AppSetting model)
        {
            var settings = db.AppSettings.Find(1);
            settings.DebtAlertThresholdILS = model.DebtAlertThresholdILS;
            settings.DebtAlertThresholdUSD = model.DebtAlertThresholdUSD;
            db.SaveChanges();

            TempData["Success"] = "تم حفظ الإعدادات بنجاح";
            return RedirectToAction("Index");
        }
    }
}
