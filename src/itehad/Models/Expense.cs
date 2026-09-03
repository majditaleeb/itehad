using System;
using System.ComponentModel.DataAnnotations;

namespace itehad.Models
{
    public class Expense
    {
        public int Id { get; set; }

        [Display(Name = "التصنيف")]
        public int CategoryId { get; set; }
        public virtual ExpenseCategory Category { get; set; }

        [Display(Name = "السائق")]
        public int? DriverId { get; set; }
        public virtual Driver Driver { get; set; }

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

        public DateTime CreatedAt { get; set; }
    }
}
