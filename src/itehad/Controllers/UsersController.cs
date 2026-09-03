using System.Linq;
using System.Web.Helpers;
using System.Web.Mvc;
using itehad.Data;
using itehad.Helpers;
using itehad.Models;
using itehad.Models.ViewModels;

namespace itehad.Controllers
{
    [Authorize(Roles = Modules.Admin)]
    public class UsersController : BaseController
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }

        public ActionResult Index()
        {
            var users = db.AppUsers.OrderByDescending(u => u.IsActive).ThenBy(u => u.Username).ToList();
            return View(users);
        }

        public ActionResult Create()
        {
            var vm = new UserFormViewModel { IsActive = true };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UserFormViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.Password))
            {
                ModelState.AddModelError("Password", "كلمة المرور مطلوبة عند إنشاء مستخدم جديد");
            }

            if (db.AppUsers.Any(u => u.Username == vm.Username))
            {
                ModelState.AddModelError("Username", "اسم المستخدم مستخدم مسبقًا");
            }

            if (!ModelState.IsValid) return View(vm);

            var user = new AppUser
            {
                Username = vm.Username.Trim(),
                PasswordHash = Crypto.HashPassword(vm.Password),
                DisplayName = vm.DisplayName,
                IsAdmin = vm.IsAdmin,
                IsActive = vm.IsActive,
                CreatedDate = System.DateTime.Now
            };

            if (!vm.IsAdmin && vm.SelectedModules != null)
            {
                foreach (var key in vm.SelectedModules.Intersect(Modules.Grantable))
                {
                    user.Modules.Add(new AppUserModule { ModuleKey = key });
                }
            }

            db.AppUsers.Add(user);
            db.SaveChanges();

            TempData["Success"] = "تمت إضافة المستخدم بنجاح";
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var user = db.AppUsers.Find(id);
            if (user == null) return HttpNotFound();

            var vm = new UserFormViewModel
            {
                Id = user.Id,
                Username = user.Username,
                DisplayName = user.DisplayName,
                IsAdmin = user.IsAdmin,
                IsActive = user.IsActive,
                SelectedModules = db.AppUserModules.Where(m => m.UserId == user.Id).Select(m => m.ModuleKey).ToArray()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UserFormViewModel vm)
        {
            var user = db.AppUsers.Find(vm.Id);
            if (user == null) return HttpNotFound();

            if (db.AppUsers.Any(u => u.Username == vm.Username && u.Id != vm.Id))
            {
                ModelState.AddModelError("Username", "اسم المستخدم مستخدم مسبقًا");
            }

            if (!ModelState.IsValid) return View(vm);

            user.Username = vm.Username.Trim();
            user.DisplayName = vm.DisplayName;
            user.IsAdmin = vm.IsAdmin;
            user.IsActive = vm.IsActive;

            if (!string.IsNullOrWhiteSpace(vm.Password))
            {
                user.PasswordHash = Crypto.HashPassword(vm.Password);
            }

            var existingModules = db.AppUserModules.Where(m => m.UserId == user.Id).ToList();
            foreach (var m in existingModules) db.AppUserModules.Remove(m);

            if (!vm.IsAdmin && vm.SelectedModules != null)
            {
                foreach (var key in vm.SelectedModules.Intersect(Modules.Grantable))
                {
                    db.AppUserModules.Add(new AppUserModule { UserId = user.Id, ModuleKey = key });
                }
            }

            db.SaveChanges();

            TempData["Success"] = "تم تعديل بيانات المستخدم بنجاح";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleActive(int id)
        {
            var user = db.AppUsers.Find(id);
            if (user == null) return HttpNotFound();

            if (user.Username == User.Identity.Name)
            {
                TempData["Error"] = "لا يمكنك تعطيل حسابك الخاص";
                return RedirectToAction("Index");
            }

            user.IsActive = !user.IsActive;
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
