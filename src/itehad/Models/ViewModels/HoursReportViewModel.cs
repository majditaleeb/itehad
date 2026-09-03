using System;
using System.Collections.Generic;

namespace itehad.Models.ViewModels
{
    public class DriverHoursReportRow
    {
        public int DriverId { get; set; }
        public string DriverName { get; set; }
        public double TotalHours { get; set; }
        public bool IsCurrentlyOnDuty { get; set; }
    }

    public class AbsenceRow
    {
        public string DriverName { get; set; }
        public List<DateTime> AbsentDates { get; set; }
    }

    public class HoursReportViewModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<DriverHoursReportRow> Rows { get; set; }
        public List<AbsenceRow> Absences { get; set; }
    }
}
