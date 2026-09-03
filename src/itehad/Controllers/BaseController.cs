using System.Linq;
using System.Web.Mvc;
using itehad.Data;

namespace itehad.Controllers
{
    public abstract class BaseController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            if (User != null && User.Identity.IsAuthenticated)
            {
                using (var db = new ApplicationDbContext())
                {
                    var user = db.AppUsers.FirstOrDefault(u => u.Username == User.Identity.Name);
                    if (user != null)
                    {
                        ViewBag.CurrentDisplayName = user.DisplayName;
                        ViewBag.CurrentIsAdmin = user.IsAdmin;
                    }
                }
            }
        }
    }
}
