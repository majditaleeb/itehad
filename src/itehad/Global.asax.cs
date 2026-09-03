using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using itehad.Data;
using itehad.Models;

namespace itehad
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            Database.SetInitializer<ApplicationDbContext>(null);
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            EnsureDefaultAdmin();
        }

        private void EnsureDefaultAdmin()
        {
            using (var db = new ApplicationDbContext())
            {
                if (db.AppUsers.Any()) return;

                db.AppUsers.Add(new AppUser
                {
                    Username = "admin",
                    PasswordHash = Crypto.HashPassword("Admin@123"),
                    DisplayName = "المدير",
                    IsAdmin = true,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                });
                db.SaveChanges();
            }
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            var authCookie = Context.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie == null) return;

            FormsAuthenticationTicket authTicket;
            try
            {
                authTicket = FormsAuthentication.Decrypt(authCookie.Value);
            }
            catch
            {
                return;
            }

            if (authTicket == null) return;

            var roles = string.IsNullOrEmpty(authTicket.UserData)
                ? new string[0]
                : authTicket.UserData.Split('|');

            var identity = new FormsIdentity(authTicket);
            Context.User = new GenericPrincipal(identity, roles);
        }
    }
}
