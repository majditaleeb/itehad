using System.ComponentModel.DataAnnotations;

namespace itehad.Models
{
    public class BookingSource
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم مصدر الحجز مطلوب")]
        [StringLength(100)]
        [Display(Name = "مصدر الحجز")]
        public string Name { get; set; }
    }
}
