using System;
using System.ComponentModel.DataAnnotations;

namespace itehad.Models
{
    public class DriverAttendance
    {
        public int Id { get; set; }

        public int DriverId { get; set; }
        public virtual Driver Driver { get; set; }

        [Display(Name = "وقت الدخول")]
        public DateTime CheckInTime { get; set; }

        [Display(Name = "وقت الخروج")]
        public DateTime? CheckOutTime { get; set; }
    }
}
