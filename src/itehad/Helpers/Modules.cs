namespace itehad.Helpers
{
    public static class Modules
    {
        public const string Admin = "Admin";
        public const string Trips = "Trips";
        public const string Customers = "Customers";
        public const string Drivers = "Drivers";
        public const string Attendance = "Attendance";
        public const string HoursReport = "HoursReport";
        public const string ProfitReport = "ProfitReport";
        public const string Settings = "Settings";
        public const string Expenses = "Expenses";

        public static readonly string[] Grantable =
        {
            Trips, Customers, Drivers, Attendance, HoursReport, ProfitReport, Settings, Expenses
        };

        public static readonly System.Collections.Generic.Dictionary<string, string> DisplayNames = new System.Collections.Generic.Dictionary<string, string>
        {
            { Trips, "الرحلات وسجل الحركة" },
            { Customers, "الزبائن وكشوف الحساب" },
            { Drivers, "السائقون" },
            { Attendance, "حضور وانصراف السائقين" },
            { HoursReport, "تقرير ساعات العمل" },
            { ProfitReport, "تقرير الأرباح" },
            { Settings, "الإعدادات العامة (المواقع، مصادر الحجز، تصنيفات المصاريف، الحدود)" },
            { Expenses, "المصاريف" }
        };
    }
}
