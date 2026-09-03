using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace itehad.Models.ViewModels
{
    public class ExpenseFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "التصنيف مطلوب")]
        [Display(Name = "التصنيف")]
        public int CategoryId { get; set; }

        [Display(Name = "السائق")]
        public int? DriverId { get; set; }

        [StringLength(100)]
        [Display(Name = "رقم الفاتورة")]
        public string InvoiceNumber { get; set; }

        [StringLength(200)]
        [Display(Name = "اسم المشتغل المرخص")]
        public string VendorName { get; set; }

        [StringLength(100)]
        [Display(Name = "رقم المشتغل المرخص")]
        public string VendorLicenseNumber { get; set; }

        [Required(ErrorMessage = "تاريخ الفاتورة مطلوب")]
        [Display(Name = "تاريخ الفاتورة")]
        public DateTime InvoiceDate { get; set; }

        [Required(ErrorMessage = "قيمة الفاتورة مطلوبة")]
        [Range(0.01, 1000000, ErrorMessage = "القيمة يجب أن تكون أكبر من صفر")]
        [Display(Name = "قيمة الفاتورة (₪)")]
        public decimal Amount { get; set; }

        [StringLength(500)]
        [Display(Name = "ملاحظات")]
        public string Notes { get; set; }

        public IEnumerable<SelectListItem> CategoryOptions { get; set; }
        public IEnumerable<SelectListItem> DriverOptions { get; set; }

        public static readonly string[] DriverRequiredCategoryNames = { "سولار", "صيانة" };
    }
}
