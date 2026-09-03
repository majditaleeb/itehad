using System;
using System.ComponentModel.DataAnnotations;

namespace itehad.Models
{
    public class CustomerPayment
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; }

        [Required(ErrorMessage = "المبلغ مطلوب")]
        [Range(0.01, 1000000, ErrorMessage = "المبلغ يجب أن يكون أكبر من صفر")]
        [Display(Name = "المبلغ")]
        public decimal Amount { get; set; }

        [Display(Name = "العملة")]
        public CurrencyType Currency { get; set; }

        [Required(ErrorMessage = "تاريخ الدفعة مطلوب")]
        [Display(Name = "تاريخ الدفعة")]
        public DateTime PaymentDate { get; set; }

        [StringLength(500)]
        [Display(Name = "ملاحظات")]
        public string Notes { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
