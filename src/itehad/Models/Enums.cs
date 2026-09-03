using System.ComponentModel.DataAnnotations;

namespace itehad.Models
{
    public enum TripRequestType : byte
    {
        [Display(Name = "ترانسفير (نقلة واحدة)")]
        Transfer = 0,

        [Display(Name = "حجز لعدة أيام")]
        MultiDay = 1
    }

    public enum CurrencyType : byte
    {
        [Display(Name = "شيقل")]
        ILS = 0,

        [Display(Name = "دولار")]
        USD = 1
    }

    public enum PaymentMethodType : byte
    {
        [Display(Name = "نقدي")]
        Cash = 0,

        [Display(Name = "ذمم")]
        Credit = 1
    }
}
