using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;

/// <summary>
/// Saves a batch of trips added on the Create page (queue mode). Each trip is
/// inserted like the app does it (cash trips settled immediately, credit trips
/// open) and receives its own permanent serial number (identity Id), which is
/// reported back to the user in the success toast.
/// </summary>
[Authorize(Roles = "Trips")]
public class TripsBatchController : Controller
{
    private static string ConnStr
    {
        get { return ConfigurationManager.ConnectionStrings["ApplicationDbContext"].ConnectionString; }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult SaveBatch(string tripsJson)
    {
        JArray items;
        try { items = JArray.Parse(tripsJson ?? "[]"); }
        catch { items = new JArray(); }

        if (items.Count == 0)
        {
            TempData["Error"] = "لا توجد رحلات في القائمة للحفظ";
            return RedirectToAction("Create", "Trips");
        }
        if (items.Count > 50)
        {
            TempData["Error"] = "الحد الأقصى 50 رحلة في الدفعة الواحدة";
            return RedirectToAction("Create", "Trips");
        }

        var newIds = new List<int>();
        DateTime firstDate = DateTime.Today;

        using (var con = new SqlConnection(ConnStr))
        {
            con.Open();
            using (var tx = con.BeginTransaction())
            {
                try
                {
                    foreach (var it in items)
                    {
                        var tripDate = DateTime.Parse((string)it["tripDate"], CultureInfo.InvariantCulture);
                        int bookingSourceId = (int)it["bookingSourceId"];
                        int customerId = (int)it["customerId"];
                        int requestType = (int?)it["requestType"] ?? 0;
                        int? daysCount = requestType == 1 ? (int?)it["daysCount"] : null;
                        int fromLocationId = (int)it["fromLocationId"];
                        int toLocationId = (int)it["toLocationId"];
                        var driverIds = ((JArray)it["driverIds"]).Select(v => (int)v).Distinct().ToList();
                        decimal fare = decimal.Parse((string)it["fare"], CultureInfo.InvariantCulture);
                        int currency = (int?)it["currency"] ?? 0;
                        int paymentMethod = (int?)it["paymentMethod"] ?? 0;
                        string notes = (string)it["notes"];

                        if (bookingSourceId <= 0 || customerId <= 0 || fromLocationId <= 0 ||
                            toLocationId <= 0 || driverIds.Count == 0 || fare < 0)
                        {
                            throw new InvalidOperationException("بيانات رحلة غير مكتملة");
                        }

                        if (newIds.Count == 0) firstDate = tripDate.Date;

                        bool cash = paymentMethod == 0;
                        int tripId;
                        using (var cmd = new SqlCommand(@"
                            INSERT INTO dbo.Trips
                                (TripDate, BookingSourceId, CustomerId, RequestType, DaysCount,
                                 FromLocationId, ToLocationId, Fare, Currency, PaymentMethod,
                                 IsSettled, SettledDate, Notes, CreatedAt)
                            VALUES
                                (@TripDate, @BookingSourceId, @CustomerId, @RequestType, @DaysCount,
                                 @FromLocationId, @ToLocationId, @Fare, @Currency, @PaymentMethod,
                                 @IsSettled, @SettledDate, @Notes, SYSDATETIME());
                            SELECT CAST(SCOPE_IDENTITY() AS int);", con, tx))
                        {
                            cmd.Parameters.AddWithValue("@TripDate", tripDate);
                            cmd.Parameters.AddWithValue("@BookingSourceId", bookingSourceId);
                            cmd.Parameters.AddWithValue("@CustomerId", customerId);
                            cmd.Parameters.AddWithValue("@RequestType", requestType);
                            cmd.Parameters.AddWithValue("@DaysCount", (object)daysCount ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@FromLocationId", fromLocationId);
                            cmd.Parameters.AddWithValue("@ToLocationId", toLocationId);
                            cmd.Parameters.AddWithValue("@Fare", fare);
                            cmd.Parameters.AddWithValue("@Currency", currency);
                            cmd.Parameters.AddWithValue("@PaymentMethod", paymentMethod);
                            cmd.Parameters.AddWithValue("@IsSettled", cash);
                            cmd.Parameters.AddWithValue("@SettledDate", cash ? (object)tripDate : DBNull.Value);
                            cmd.Parameters.AddWithValue("@Notes", string.IsNullOrWhiteSpace(notes) ? (object)DBNull.Value : notes.Trim());
                            tripId = (int)cmd.ExecuteScalar();
                        }

                        foreach (var did in driverIds)
                        {
                            using (var cmd = new SqlCommand(
                                "INSERT INTO dbo.TripDrivers (TripId, DriverId) VALUES (@t, @d)", con, tx))
                            {
                                cmd.Parameters.AddWithValue("@t", tripId);
                                cmd.Parameters.AddWithValue("@d", did);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        newIds.Add(tripId);
                    }

                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    TempData["Error"] = "تعذّر حفظ الرحلات — تأكد من اكتمال بيانات كل رحلة (المصدر، الزبون، المواقع، سائق واحد على الأقل)";
                    return RedirectToAction("Create", "Trips");
                }
            }
        }

        TempData["Success"] = "تم حفظ " + newIds.Count + " رحلة بنجاح — الأرقام التسلسلية: " +
                              string.Join("، ", newIds.Select(n => "#" + n));
        return RedirectToAction("Index", "Trips", new { date = firstDate.ToString("yyyy-MM-dd") });
    }
}
