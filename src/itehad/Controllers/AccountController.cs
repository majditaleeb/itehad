using System;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;
using itehad.Data;
using itehad.Helpers;
using itehad.Models.ViewModels;

namespace itehad.Controllers
{
    [AllowAnonymous]
    public class AccountController : BaseController
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }

        public ActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel vm, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View(vm);
            }

            var user = db.AppUsers.FirstOrDefault(u => u.Username == vm.Username && u.IsActive);
            if (user == null || !Crypto.VerifyHashedPassword(user.PasswordHash, vm.Password))
            {
                ModelState.AddModelError("", "اسم المستخدم أو كلمة المرور غير صحيحة");
                ViewBag.ReturnUrl = returnUrl;
                return View(vm);
            }

            var moduleKeys = user.IsAdmin
                ? new[] { Modules.Admin }.Concat(Modules.Grantable).ToArray()
                : db.AppUserModules.Where(m => m.UserId == user.Id).Select(m => m.ModuleKey).ToArray();

            var userData = string.Join("|", moduleKeys);

            var ticket = new FormsAuthenticationTicket(
                1, user.Username, DateTime.Now, DateTime.Now.AddMinutes(480), false, userData);

            var encryptedTicket = FormsAuthentication.Encrypt(ticket);
            var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket) { HttpOnly = true };
            Response.Cookies.Add(authCookie);

            if (Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }
    }
}
