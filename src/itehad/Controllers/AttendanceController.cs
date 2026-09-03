using System;
using System.Linq;
using System.Web.Mvc;
using itehad.Data;
using itehad.Helpers;
using itehad.Models;
using itehad.Models.ViewModels;

namespace itehad.Controllers
{
    [Authorize(Roles = Modules.Attendance)]
    public class AttendanceController : BaseController
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }

        public ActionResult Index()
        {
            var drivers = db.Drivers.Where(d => d.IsActive).OrderBy(d => d.Name).ToList();
            var openRecords = db.DriverAttendances.Where(a => a.CheckOutTime == null).ToList();

            var vm = drivers.Select(d =>
            {
                var open = openRecords.FirstOrDefault(a => a.DriverId == d.Id);
                return new DriverAttendanceStatusViewModel
                {
                    DriverId = d.Id,
                    DriverName = d.Name,
                    CarNumber = d.CarNumber,
                    IsOnDuty = open != null,
                    OpenAttendanceId = open?.Id,
                    CheckInTime = open?.CheckInTime
                };
            }).ToList();

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CheckIn(int driverId, string time)
        {
            bool alreadyOpen = db.DriverAttendances.Any(a => a.DriverId == driverId && a.CheckOutTime == null);
            if (!alreadyOpen)
            {
                db.DriverAttendances.Add(new DriverAttendance { DriverId = driverId, CheckInTime = CombineTodayWithTime(time) });
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CheckOut(int attendanceId, string time)
        {
            var record = db.DriverAttendances.Find(attendanceId);
            if (record != null && record.CheckOutTime == null)
            {
                record.CheckOutTime = CombineTodayWithTime(time);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        private static DateTime CombineTodayWithTime(string time)
        {
            if (TimeSpan.TryParse(time, out var timeOfDay))
            {
                return DateTime.Today.Add(timeOfDay);
            }
            return DateTime.Now;
        }
    }
}
