using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Routing;

namespace itehad.Models.ViewModels
{
    /// <summary>
    /// بحث عمود-بعمود فوق جدول سجل الحركة. بينطبق على الاستعلام قبل العدّ وقبل
    /// جمع الكروت، فأرقام الكروت وعدّاد النتائج دايمًا بيطابقوا اللي ظاهر بالجدول،
    /// وبيغطّي الفترة كلها مش بس الصفحة المعروضة.
    ///
    /// الأسماء قصيرة ومختلفة عن بارامترات الأكشن (date / from / to / page) عشان
    /// ما يتلخبط الـ model binder، لأنه بيربط بدون بادئة.
    /// </summary>
    public class TripFilter
    {
        public string Num { get; set; }      // رقم الرحلة — مطابقة تامة
        public string Day { get; set; }      // سنة / سنة-شهر / سنة-شهر-يوم — أو يوم/شهر/سنة
        public string Time { get; set; }     // "10" أو "10:26"
        public string Src { get; set; }      // مصدر الحجز — يحتوي
        public string Cust { get; set; }     // الزبون — يحتوي
        public string Route { get; set; }    // من أو إلى — يحتوي
        public string Drv { get; set; }      // أي سائق على الرحلة — يحتوي
        public string Fare { get; set; }     // الأجرة — مطابقة تامة
        public string Pay { get; set; }      // "0" نقدي / "1" ذمم
        public string Note { get; set; }     // ملاحظات — يحتوي

        /// <summary>مدخل غير صالح = ما في نتائج. Id بيبدأ من 1 فهاد الشرط بينترجم لـ SQL وبيرجع فاضي دايماً.</summary>
        private static IQueryable<Trip> None(IQueryable<Trip> q) { return q.Where(t => t.Id < 0); }

        private static string Clean(string s)
        {
            return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
        }

        public bool IsEmpty { get { return RouteValues().Count == 0; } }

        /// <summary>القيم غير الفاضية، جاهزة تنحط على أي رابط (صفحات، طباعة، تصدير).</summary>
        public IDictionary<string, object> RouteValues()
        {
            var d = new Dictionary<string, object>();
            Add(d, "num", Num);
            Add(d, "day", Day);
            Add(d, "time", Time);
            Add(d, "src", Src);
            Add(d, "cust", Cust);
            Add(d, "route", Route);
            Add(d, "drv", Drv);
            Add(d, "fare", Fare);
            Add(d, "pay", Pay);
            Add(d, "note", Note);
            return d;
        }

        private static void Add(IDictionary<string, object> d, string key, string value)
        {
            var v = Clean(value);
            if (v != null) { d[key] = v; }
        }

        /// <summary>قيم رابط جاهزة: اللي بعتناه + بحث الأعمدة الحالي فوقه.</summary>
        public RouteValueDictionary With(object extra)
        {
            var rv = new RouteValueDictionary(extra);
            foreach (var kv in RouteValues()) { rv[kv.Key] = kv.Value; }
            return rv;
        }

        // ------------------------------ EF ------------------------------

        public IQueryable<Trip> Apply(IQueryable<Trip> q)
        {
            var num = Clean(Num);
            if (num != null)
            {
                int id;
                q = int.TryParse(num, NumberStyles.Integer, CultureInfo.InvariantCulture, out id)
                    ? q.Where(t => t.Id == id)
                    : None(q);
            }

            var day = Clean(Day);
            if (day != null)
            {
                DateTime dayStart, dayEnd;
                q = TryParseDay(day, out dayStart, out dayEnd)
                    ? q.Where(t => t.TripDate >= dayStart && t.TripDate < dayEnd)
                    : None(q);
            }

            var time = Clean(Time);
            if (time != null)
            {
                int hh, mm;
                if (TryParseTime(time, out hh, out mm))
                {
                    q = q.Where(t => t.TripDate.Hour == hh);
                    if (mm >= 0) { q = q.Where(t => t.TripDate.Minute == mm); }
                }
                else { q = None(q); }
            }

            var src = Clean(Src);
            if (src != null) { q = q.Where(t => t.BookingSource.Name.Contains(src)); }

            var cust = Clean(Cust);
            if (cust != null) { q = q.Where(t => t.Customer.Name.Contains(cust)); }

            var route = Clean(Route);
            if (route != null)
            {
                q = q.Where(t => t.FromLocation.Name.Contains(route) || t.ToLocation.Name.Contains(route));
            }

            var drv = Clean(Drv);
            if (drv != null) { q = q.Where(t => t.TripDrivers.Any(td => td.Driver.Name.Contains(drv))); }

            var fare = Clean(Fare);
            if (fare != null)
            {
                decimal f;
                q = decimal.TryParse(fare, NumberStyles.Any, CultureInfo.InvariantCulture, out f)
                    ? q.Where(t => t.Fare == f)
                    : None(q);
            }

            var pay = Clean(Pay);
            if (pay == "0") { q = q.Where(t => t.PaymentMethod == PaymentMethodType.Cash); }
            else if (pay == "1") { q = q.Where(t => t.PaymentMethod == PaymentMethodType.Credit); }

            var note = Clean(Note);
            if (note != null) { q = q.Where(t => t.Notes != null && t.Notes.Contains(note)); }

            return q;
        }

        // ------------------------------ SQL ------------------------------
        // تصدير Excel بيشتغل بـ SQL خام (App_Code/ExcelController)، فلازم نفس
        // الفلترة بالضبط تنطبق هناك كمان — وإلا الملف المصدَّر بيطلع غير اللي
        // على الشاشة. الأسماء المستعارة لازم تطابق استعلام التصدير.

        public const string SqlTrip = "t";
        public const string SqlSource = "bs";
        public const string SqlCustomer = "c";
        public const string SqlFrom = "fl";
        public const string SqlTo = "tl";

        /// <summary>
        /// بيرجّع شروط إضافية جاهزة للصق بعد WHERE الموجود (بتبدأ بـ " AND ")،
        /// وبيعبّي <paramref name="args"/> بقيم البارامترات.
        /// </summary>
        public string SqlWhere(IDictionary<string, object> args)
        {
            var sb = new StringBuilder();
            var i = 0;
            Func<object, string> p = value =>
            {
                var name = "@f" + (i++).ToString(CultureInfo.InvariantCulture);
                args[name] = value;
                return name;
            };
            Action<string> and = clause => sb.Append(" AND ").Append(clause);

            var num = Clean(Num);
            if (num != null)
            {
                int id;
                if (int.TryParse(num, NumberStyles.Integer, CultureInfo.InvariantCulture, out id))
                    and(SqlTrip + ".Id = " + p(id));
                else and("1 = 0");
            }

            var day = Clean(Day);
            if (day != null)
            {
                DateTime dayStart, dayEnd;
                if (TryParseDay(day, out dayStart, out dayEnd))
                    and(SqlTrip + ".TripDate >= " + p(dayStart) + " AND " + SqlTrip + ".TripDate < " + p(dayEnd));
                else and("1 = 0");
            }

            var time = Clean(Time);
            if (time != null)
            {
                int hh, mm;
                if (TryParseTime(time, out hh, out mm))
                {
                    and("DATEPART(hour, " + SqlTrip + ".TripDate) = " + p(hh));
                    if (mm >= 0) and("DATEPART(minute, " + SqlTrip + ".TripDate) = " + p(mm));
                }
                else and("1 = 0");
            }

            var src = Clean(Src);
            if (src != null) and(SqlSource + ".Name LIKE " + p(Like(src)));

            var cust = Clean(Cust);
            if (cust != null) and(SqlCustomer + ".Name LIKE " + p(Like(cust)));

            var route = Clean(Route);
            if (route != null)
            {
                var like = p(Like(route));
                and("(" + SqlFrom + ".Name LIKE " + like + " OR " + SqlTo + ".Name LIKE " + like + ")");
            }

            var drv = Clean(Drv);
            if (drv != null)
            {
                and("EXISTS (SELECT 1 FROM dbo.TripDrivers td2 JOIN dbo.Drivers dr2 ON dr2.Id = td2.DriverId" +
                    " WHERE td2.TripId = " + SqlTrip + ".Id AND dr2.Name LIKE " + p(Like(drv)) + ")");
            }

            var fare = Clean(Fare);
            if (fare != null)
            {
                decimal f;
                if (decimal.TryParse(fare, NumberStyles.Any, CultureInfo.InvariantCulture, out f))
                    and(SqlTrip + ".Fare = " + p(f));
                else and("1 = 0");
            }

            var pay = Clean(Pay);
            if (pay == "0") and(SqlTrip + ".PaymentMethod = 0");
            else if (pay == "1") and(SqlTrip + ".PaymentMethod = 1");

            var note = Clean(Note);
            if (note != null) and(SqlTrip + ".Notes LIKE " + p(Like(note)));

            return sb.ToString();
        }

        /// <summary>هروب محارف LIKE عشان % و _ و [ تنبحث كنص عادي.</summary>
        private static string Like(string term)
        {
            return "%" + term.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]") + "%";
        }

        // ---------------------------- Parsing ----------------------------

        private static readonly string[] DayFormats =
        {
            "yyyy-MM-dd", "yyyy/MM/dd", "yyyy-M-d", "yyyy/M/d",
            "dd/MM/yyyy", "dd-MM-yyyy", "d/M/yyyy", "d-M-yyyy"
        };

        private static readonly string[] MonthFormats = { "yyyy-MM", "yyyy/MM", "yyyy-M", "yyyy/M" };

        /// <summary>سنة، أو سنة-شهر، أو تاريخ كامل (بأي من الصيغتين).</summary>
        private static bool TryParseDay(string s, out DateTime start, out DateTime endExclusive)
        {
            var inv = CultureInfo.InvariantCulture;
            DateTime d;

            if (DateTime.TryParseExact(s, DayFormats, inv, DateTimeStyles.None, out d))
            {
                start = d.Date;
                endExclusive = start.AddDays(1);
                return true;
            }

            if (DateTime.TryParseExact(s, MonthFormats, inv, DateTimeStyles.None, out d))
            {
                start = new DateTime(d.Year, d.Month, 1);
                endExclusive = start.AddMonths(1);
                return true;
            }

            int year;
            if (s.Length == 4 && int.TryParse(s, NumberStyles.None, inv, out year) && year >= 1900 && year <= 2999)
            {
                start = new DateTime(year, 1, 1);
                endExclusive = start.AddYears(1);
                return true;
            }

            start = endExclusive = DateTime.MinValue;
            return false;
        }

        /// <summary>"10" = الساعة العاشرة كلها، "10:26" = الدقيقة بالضبط.</summary>
        private static bool TryParseTime(string s, out int hour, out int minute)
        {
            hour = minute = -1;
            var m = Regex.Match(s, @"^(\d{1,2})(?:\s*:\s*(\d{1,2}))?$");
            if (!m.Success) { return false; }

            hour = int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
            if (hour > 23) { return false; }

            if (m.Groups[2].Success)
            {
                minute = int.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture);
                if (minute > 59) { return false; }
            }
            return true;
        }
    }
}
