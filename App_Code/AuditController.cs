using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;

/// <summary>
/// Trip audit log + trip deletion. Implemented in App_Code (compiled by ASP.NET
/// at runtime) so no changes to the compiled itehad.dll are required.
///
/// - Index      : shows the change/delete history recorded by the database
///                triggers TR_Trips_Audit_Update / TR_Trips_Audit_Delete.
/// - DeleteTrip : deletes a trip (cascade removes its drivers) and records who
///                did it. The delete trigger writes the audit row automatically.
/// </summary>
[Authorize(Roles = "Trips")]
public class AuditController : Controller
{
    private static string ConnStr
    {
        get { return ConfigurationManager.ConnectionStrings["ApplicationDbContext"].ConnectionString; }
    }

    // GET /Audit/Index  -> change & delete history
    public ActionResult Index()
    {
        var table = new DataTable();
        using (var con = new SqlConnection(ConnStr))
        using (var da = new SqlDataAdapter(
            "SELECT TOP 1000 * FROM dbo.TripAuditLog ORDER BY Id DESC", con))
        {
            da.Fill(table);
        }
        return View(table);
    }

    // POST /Audit/DeleteTrip  -> delete one trip (called from the daily log)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult DeleteTrip(int id, string date)
    {
        using (var con = new SqlConnection(ConnStr))
        {
            con.Open();

            // Record who is performing the delete so the trigger can log it.
            SetSessionContext(con, "app_user", User.Identity.Name);

            // Capture the driver names before the cascade removes them.
            string drivers = null;
            using (var cmd = new SqlCommand(
                "SELECT STRING_AGG(dr.Name, N'، ') " +
                "FROM dbo.TripDrivers td JOIN dbo.Drivers dr ON dr.Id = td.DriverId " +
                "WHERE td.TripId = @id", con))
            {
                cmd.Parameters.AddWithValue("@id", id);
                var result = cmd.ExecuteScalar();
                drivers = (result == null || result == DBNull.Value) ? null : (string)result;
            }
            SetSessionContext(con, "trip_drivers", drivers);

            // Delete the trip. FK cascade removes TripDrivers; the AFTER DELETE
            // trigger writes the audit row using the session context values above.
            using (var cmd = new SqlCommand("DELETE FROM dbo.Trips WHERE Id = @id", con))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        TempData["Success"] = "تم حذف الرحلة وتسجيلها في سجل التعديلات";

        DateTime parsed;
        if (DateTime.TryParse(date, out parsed))
            return RedirectToAction("Index", "Trips", new { date = parsed.ToString("yyyy-MM-dd") });

        return RedirectToAction("Index", "Trips");
    }

    private static void SetSessionContext(SqlConnection con, string key, string value)
    {
        using (var cmd = new SqlCommand("EXEC sp_set_session_context @k, @v", con))
        {
            cmd.Parameters.AddWithValue("@k", key);
            cmd.Parameters.AddWithValue("@v", (object)value ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }
    }
}
