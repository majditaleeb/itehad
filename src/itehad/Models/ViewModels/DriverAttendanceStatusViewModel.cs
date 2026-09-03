using System;

namespace itehad.Models.ViewModels
{
    public class DriverAttendanceStatusViewModel
    {
        public int DriverId { get; set; }
        public string DriverName { get; set; }
        public string CarNumber { get; set; }
        public bool IsOnDuty { get; set; }
        public int? OpenAttendanceId { get; set; }
        public DateTime? CheckInTime { get; set; }
    }
}
