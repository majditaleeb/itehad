namespace itehad.Models
{
    public class TripDriver
    {
        public int TripId { get; set; }
        public virtual Trip Trip { get; set; }

        public int DriverId { get; set; }
        public virtual Driver Driver { get; set; }
    }
}
