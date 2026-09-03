using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace itehad.Models.ViewModels
{
    public class TripFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "التاريخ والوقت مطلوب")]
        [Display(Name = "التاريخ والوقت")]
        public DateTime TripDate { get; set; }

        [Required(ErrorMessage = "مصدر الحجز مطلوب")]
        [Display(Name = "مصدر الحجز")]
        public int BookingSourceId { get; set; }

        [Required(ErrorMessage = "الزبون مطلوب")]
        [Display(Name = "الزبون")]
        public int CustomerId { get; set; }

        [Display(Name = "حالة الطلب")]
        public TripRequestType RequestType { get; set; }

        [Display(Name = "عدد الأيام")]
        [Range(1, 3650, ErrorMessage = "عدد الأيام غير صحيح")]
        public int? DaysCount { get; set; }

        [Required(ErrorMessage = "موقع الانطلاق مطلوب")]
        [Display(Name = "من")]
        public int FromLocationId { get; set; }

        [Required(ErrorMessage = "موقع الوجهة مطلوب")]
        [Display(Name = "إلى")]
        public int ToLocationId { get; set; }

        [Required(ErrorMessage = "يجب اختيار سائق واحد على الأقل")]
        [Display(Name = "السائقون / السيارات")]
        public int[] DriverIds { get; set; }

        [Required(ErrorMessage = "الأجرة مطلوبة")]
        [Range(0.01, 1000000, ErrorMessage = "الأجرة يجب أن تكون أكبر من صفر")]
        [Display(Name = "الأجرة")]
        public decimal Fare { get; set; }

        [Display(Name = "العملة")]
        public CurrencyType Currency { get; set; }

        [Display(Name = "طريقة الدفع")]
        public PaymentMethodType PaymentMethod { get; set; }

        [StringLength(500)]
        [Display(Name = "ملاحظات")]
        public string Notes { get; set; }

        public IEnumerable<SelectListItem> BookingSourceOptions { get; set; }
        public IEnumerable<SelectListItem> CustomerOptions { get; set; }
        public IEnumerable<SelectListItem> LocationOptions { get; set; }
        public IEnumerable<SelectListItem> DriverOptions { get; set; }
    }
}
