using System;
using System.ComponentModel.DataAnnotations;

namespace itehad.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم الزبون مطلوب")]
        [StringLength(200)]
        [Display(Name = "اسم الزبون")]
        public string Name { get; set; }

        [StringLength(30)]
        [Display(Name = "رقم الهاتف")]
        public string Phone { get; set; }

        [Display(Name = "تاريخ الإضافة")]
        public DateTime CreatedDate { get; set; }
    }
}
