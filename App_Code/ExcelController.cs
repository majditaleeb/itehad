using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

/// <summary>
/// Real .xlsx exports (styled, RTL, native date/number cells) replacing the
/// plain CSV exports. Lives in App_Code so the compiled itehad.dll is untouched.
/// </summary>
public class ExcelController : Controller
{
    private const string XlsxMime = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private static string ConnStr
    {
        get { return ConfigurationManager.ConnectionStrings["ApplicationDbContext"].ConnectionString; }
    }

    private static DataTable Query(string sql, params SqlParameter[] ps)
    {
        var table = new DataTable();
        using (var con = new SqlConnection(ConnStr))
        using (var da = new SqlDataAdapter(sql, con))
        {
            da.SelectCommand.Parameters.AddRange(ps);
            da.Fill(table);
        }
        return table;
    }

    // ============================ Daily trips log ============================
    // GET /Excel/ExportTrips?date=yyyy-MM-dd
    // GET /Excel/ExportTrips?from=yyyy-MM-dd&to=yyyy-MM-dd   (inclusive range)
    [Authorize(Roles = "Trips")]
    public ActionResult ExportTrips(DateTime? date, DateTime? from, DateTime? to)
    {
        DateTime start, end;
        if (from.HasValue || to.HasValue)
        {
            start = (from ?? to.Value).Date;
            end = (to ?? from.Value).Date;
            if (end < start) { var swap = start; start = end; end = swap; }
        }
        else
        {
            start = end = (date ?? DateTime.Today).Date;
        }

        bool isRange = start != end;

        var rows = Query(@"
            SELECT t.Id, t.TripDate, bs.Name AS Source, c.Name AS Customer,
                   fl.Name AS FromLoc, tl.Name AS ToLoc,
                   ISNULL((SELECT STRING_AGG(CAST(dr.Name AS nvarchar(max)), N'، ')
                           FROM dbo.TripDrivers td JOIN dbo.Drivers dr ON dr.Id = td.DriverId
                           WHERE td.TripId = t.Id), N'') AS Drivers,
                   t.Fare, t.Currency, t.PaymentMethod, ISNULL(t.Notes, N'') AS Notes
            FROM dbo.Trips t
                JOIN dbo.BookingSources bs ON bs.Id = t.BookingSourceId
                JOIN dbo.Customers c ON c.Id = t.CustomerId
                JOIN dbo.Locations fl ON fl.Id = t.FromLocationId
                JOIN dbo.Locations tl ON tl.Id = t.ToLocationId
            WHERE t.TripDate >= @d AND t.TripDate < @d2
            ORDER BY t.TripDate",
            new SqlParameter("@d", start), new SqlParameter("@d2", end.AddDays(1)));

        var x = new Xlsx("سجل الحركة");
        if (isRange) x.SetColumns(8, 12, 9, 16, 18, 14, 14, 24, 12, 9, 9, 30);
        else x.SetColumns(8, 9, 16, 18, 14, 14, 24, 12, 9, 9, 30);
        int COLS = isRange ? 12 : 11;

        int r = x.AddRow(34, Xlsx.T("تكسي الاتحاد — سجل الحركة" + (isRange ? "" : " اليومية"), Xlsx.StTitle));
        x.Merge(r, 1, COLS);
        var period = isRange
            ? "الفترة: من " + start.ToString("yyyy-MM-dd") + " إلى " + end.ToString("yyyy-MM-dd")
            : "التاريخ: " + start.ToString("yyyy-MM-dd");
        r = x.AddRow(22, Xlsx.T(period + "   •   عدد الرحلات: " + rows.Rows.Count, Xlsx.StSubtitle));
        x.Merge(r, 1, COLS);

        var header = new List<Xlsx.Cell> { Xlsx.T("الرقم", Xlsx.StHeader) };
        if (isRange) header.Add(Xlsx.T("التاريخ", Xlsx.StHeader));
        header.AddRange(new[] {
            Xlsx.T("الوقت", Xlsx.StHeader), Xlsx.T("المصدر", Xlsx.StHeader),
            Xlsx.T("الزبون", Xlsx.StHeader), Xlsx.T("من", Xlsx.StHeader), Xlsx.T("إلى", Xlsx.StHeader),
            Xlsx.T("السائقون", Xlsx.StHeader), Xlsx.T("الأجرة", Xlsx.StHeader), Xlsx.T("العملة", Xlsx.StHeader),
            Xlsx.T("الدفع", Xlsx.StHeader), Xlsx.T("ملاحظات", Xlsx.StHeader) });
        x.AddRow(24, header.ToArray());
        x.FreezeAfterRow(3);

        decimal cashIls = 0, cashUsd = 0, creditIls = 0, creditUsd = 0;
        int i = 0;
        foreach (DataRow row in rows.Rows)
        {
            bool z = (i++ % 2) == 1;
            var fare = Convert.ToDecimal(row["Fare"]);
            bool usd = Convert.ToInt32(row["Currency"]) == 1;
            bool credit = Convert.ToInt32(row["PaymentMethod"]) == 1;
            if (credit) { if (usd) creditUsd += fare; else creditIls += fare; }
            else { if (usd) cashUsd += fare; else cashIls += fare; }

            var when = Convert.ToDateTime(row["TripDate"]);
            var cells = new List<Xlsx.Cell> {
                Xlsx.N(Convert.ToDecimal(row["Id"]), z ? Xlsx.StSerialZ : Xlsx.StSerial) };
            if (isRange) cells.Add(Xlsx.D(when, z ? Xlsx.StDateZ : Xlsx.StDate));
            cells.AddRange(new[] {
                Xlsx.D(when, z ? Xlsx.StTimeZ : Xlsx.StTime),
                Xlsx.T((string)row["Source"], z ? Xlsx.StTextZ : Xlsx.StText),
                Xlsx.T((string)row["Customer"], z ? Xlsx.StTextZ : Xlsx.StText),
                Xlsx.T((string)row["FromLoc"], z ? Xlsx.StTextZ : Xlsx.StText),
                Xlsx.T((string)row["ToLoc"], z ? Xlsx.StTextZ : Xlsx.StText),
                Xlsx.T((string)row["Drivers"], z ? Xlsx.StTextZ : Xlsx.StText),
                Xlsx.N(fare, z ? Xlsx.StMoneyZ : Xlsx.StMoney),
                Xlsx.T(usd ? "دولار" : "شيقل", z ? Xlsx.StCenterZ : Xlsx.StCenter),
                Xlsx.T(credit ? "ذمم" : "نقدي", z ? Xlsx.StCenterZ : Xlsx.StCenter),
                Xlsx.T((string)row["Notes"], z ? Xlsx.StTextZ : Xlsx.StText) });
            x.AddRow(20, cells.ToArray());
        }

        x.AddRow(8);
        AddSummary(x, "إجمالي النقدي (شيقل)", cashIls);
        AddSummary(x, "إجمالي النقدي (دولار)", cashUsd);
        AddSummary(x, "إجمالي الذمم (شيقل)", creditIls);
        AddSummary(x, "إجمالي الذمم (دولار)", creditUsd);

        var name = "سجل-الحركة-" + start.ToString("yyyy-MM-dd")
                 + (isRange ? "_" + end.ToString("yyyy-MM-dd") : "") + ".xlsx";
        return File(x.Build(), XlsxMime, name);
    }

    private static void AddSummary(Xlsx x, string label, decimal value)
    {
        int r = x.AddRow(20,
            Xlsx.T(label, Xlsx.StSumLabel), Xlsx.E(Xlsx.StSumLabel), Xlsx.E(Xlsx.StSumLabel),
            Xlsx.N(value, Xlsx.StSumValue));
        x.Merge(r, 1, 3);
    }

    // =========================== Customer statement ==========================
    // GET /Excel/ExportStatement?id=..&from=yyyy-MM-dd&to=yyyy-MM-dd
    [Authorize(Roles = "Customers")]
    public ActionResult ExportStatement(int id, DateTime? from, DateTime? to)
    {
        var toEnd = (to ?? DateTime.Today).Date.AddDays(1); // inclusive end

        string customerName;
        using (var con = new SqlConnection(ConnStr))
        using (var cmd = new SqlCommand("SELECT Name FROM dbo.Customers WHERE Id=@id", con))
        {
            cmd.Parameters.AddWithValue("@id", id);
            con.Open();
            customerName = (string)cmd.ExecuteScalar();
        }

        // One union of debit (credit-payment trips) and credit (payments) rows.
        var all = Query(@"
            SELECT t.TripDate AS D, N'رحلة: ' + fl.Name + N' ← ' + tl.Name AS Descr,
                   t.Fare AS Debit, CAST(0 AS decimal(18,2)) AS Credit, t.Currency AS Cur
            FROM dbo.Trips t
                JOIN dbo.Locations fl ON fl.Id = t.FromLocationId
                JOIN dbo.Locations tl ON tl.Id = t.ToLocationId
            WHERE t.CustomerId = @id AND t.PaymentMethod = 1
            UNION ALL
            SELECT p.PaymentDate, CASE WHEN p.Notes IS NULL OR p.Notes = N'' THEN N'دفعة' ELSE p.Notes END,
                   CAST(0 AS decimal(18,2)), p.Amount, p.Currency
            FROM dbo.CustomerPayments p
            WHERE p.CustomerId = @id
            ORDER BY D",
            new SqlParameter("@id", id));

        var x = new Xlsx("كشف حساب");
        x.SetColumns(18, 40, 13, 13, 14);
        const int COLS = 5;

        int r = x.AddRow(34, Xlsx.T("تكسي الاتحاد — كشف حساب: " + customerName, Xlsx.StTitle));
        x.Merge(r, 1, COLS);
        var period = (from.HasValue ? "من " + from.Value.ToString("yyyy-MM-dd") + " " : "") +
                     "حتى " + toEnd.AddDays(-1).ToString("yyyy-MM-dd");
        r = x.AddRow(22, Xlsx.T("الفترة: " + period + "   •   أُنشئ في: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm"), Xlsx.StSubtitle));
        x.Merge(r, 1, COLS);

        BuildLedgerSection(x, all, 0, "كشف الحساب بالشيقل (₪)", from, toEnd, COLS);
        x.AddRow(8);
        BuildLedgerSection(x, all, 1, "كشف الحساب بالدولار ($)", from, toEnd, COLS);

        var name = "كشف-حساب-" + customerName.Replace(" ", "-") + ".xlsx";
        return File(x.Build(), XlsxMime, name);
    }

    private static void BuildLedgerSection(Xlsx x, DataTable all, int currency, string title,
                                           DateTime? from, DateTime toEnd, int cols)
    {
        var rows = all.Rows.Cast<DataRow>()
            .Where(r0 => Convert.ToInt32(r0["Cur"]) == currency)
            .ToList();

        // Opening balance: net of everything before the from-date.
        decimal opening = 0;
        if (from.HasValue)
        {
            opening = rows.Where(r0 => Convert.ToDateTime(r0["D"]) < from.Value.Date)
                          .Sum(r0 => Convert.ToDecimal(r0["Debit"]) - Convert.ToDecimal(r0["Credit"]));
        }

        var visible = rows
            .Where(r0 =>
            {
                var d = Convert.ToDateTime(r0["D"]);
                return (!from.HasValue || d >= from.Value.Date) && d < toEnd;
            })
            .ToList();

        int r = x.AddRow(26, Xlsx.T(title, Xlsx.StSection));
        x.Merge(r, 1, cols);

        x.AddRow(22,
            Xlsx.T("التاريخ", Xlsx.StHeader), Xlsx.T("البيان", Xlsx.StHeader),
            Xlsx.T("مدين", Xlsx.StHeader), Xlsx.T("دائن", Xlsx.StHeader), Xlsx.T("الرصيد", Xlsx.StHeader));

        r = x.AddRow(20,
            Xlsx.T("رصيد افتتاحي", Xlsx.StSumLabel), Xlsx.E(Xlsx.StSumLabel), Xlsx.E(Xlsx.StSumLabel),
            Xlsx.E(Xlsx.StSumLabel), Xlsx.N(opening, Xlsx.StSumValue));
        x.Merge(r, 1, 4);

        decimal balance = opening;
        int i = 0;
        foreach (var row in visible)
        {
            bool z = (i++ % 2) == 1;
            var debit = Convert.ToDecimal(row["Debit"]);
            var credit = Convert.ToDecimal(row["Credit"]);
            balance += debit - credit;

            x.AddRow(20,
                Xlsx.D(Convert.ToDateTime(row["D"]), z ? Xlsx.StDateZ : Xlsx.StDate),
                Xlsx.T((string)row["Descr"], z ? Xlsx.StTextZ : Xlsx.StText),
                Xlsx.N(debit > 0 ? debit : (decimal?)null, z ? Xlsx.StDebitZ : Xlsx.StDebit),
                Xlsx.N(credit > 0 ? credit : (decimal?)null, z ? Xlsx.StCreditZ : Xlsx.StCredit),
                Xlsx.N(balance, Xlsx.StBalance));
        }

        r = x.AddRow(20,
            Xlsx.T("رصيد ختامي", Xlsx.StSumLabel), Xlsx.E(Xlsx.StSumLabel), Xlsx.E(Xlsx.StSumLabel),
            Xlsx.E(Xlsx.StSumLabel), Xlsx.N(balance, Xlsx.StSumValue));
        x.Merge(r, 1, 4);
    }
}
