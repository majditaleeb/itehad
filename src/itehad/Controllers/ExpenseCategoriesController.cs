using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web.Mvc;
using itehad.Data;
using itehad.Helpers;
using itehad.Models;

namespace itehad.Controllers
{
    [Authorize(Roles = Modules.Settings)]
    public class ExpenseCategoriesController : BaseController
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }

        public ActionResult Index()
        {
            var categories = db.ExpenseCategories.OrderBy(c => c.Name).ToList();
            return View(categories);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ExpenseCategory category)
        {
            if (!string.IsNullOrWhiteSpace(category?.Name))
            {
                db.ExpenseCategories.Add(new ExpenseCategory { Name = category.Name.Trim() });
                db.SaveChanges();
                TempData["Success"] = "تمت إضافة التصنيف بنجاح";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ExpenseCategory category)
        {
            var existing = db.ExpenseCategories.Find(category.Id);
            if (existing != null && !string.IsNullOrWhiteSpace(category.Name))
            {
                existing.Name = category.Name.Trim();
                db.SaveChanges();
                TempData["Success"] = "تم تعديل التصنيف بنجاح";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var category = db.ExpenseCategories.Find(id);
            if (category != null)
            {
                db.ExpenseCategories.Remove(category);
                try
                {
                    db.SaveChanges();
                    TempData["Success"] = "تم حذف التصنيف بنجاح";
                }
                catch (DbUpdateException)
                {
                    TempData["Error"] = "لا يمكن حذف هذا التصنيف لأنه مستخدم في مصاريف سابقة";
                }
            }
            return RedirectToAction("Index");
        }
    }
}
