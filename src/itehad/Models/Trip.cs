using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace itehad.Models
{
    public class Trip
    {
        public int Id { get; set; }

        [Display(Name = "التاريخ والوقت")]
        public DateTime TripDate { get; set; }

        [Display(Name = "مصدر الحجز")]
        public int BookingSourceId { get; set; }
        public virtual BookingSource BookingSource { get; set; }

        [Display(Name = "الزبون")]
        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; }

        [Display(Name = "حالة الطلب")]
        public TripRequestType RequestType { get; set; }

        [Display(Name = "عدد الأيام")]
        public int? DaysCount { get; set; }

        [Display(Name = "من")]
        public int FromLocationId { get; set; }
        public virtual Location FromLocation { get; set; }

        [Display(Name = "إلى")]
        public int ToLocationId { get; set; }
        public virtual Location ToLocation { get; set; }

        [Display(Name = "الأجرة")]
        public decimal Fare { get; set; }

        [Display(Name = "العملة")]
        public CurrencyType Currency { get; set; }

        [Display(Name = "طريقة الدفع")]
        public PaymentMethodType PaymentMethod { get; set; }

        [Display(Name = "تم التحصيل")]
        public bool IsSettled { get; set; }

        [Display(Name = "تاريخ التحصيل")]
        public DateTime? SettledDate { get; set; }

        [StringLength(500)]
        [Display(Name = "ملاحظات")]
        public string Notes { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual ICollection<TripDriver> TripDrivers { get; set; } = new List<TripDriver>();
    }
}
