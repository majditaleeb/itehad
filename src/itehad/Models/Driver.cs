using System.ComponentModel.DataAnnotations;

namespace itehad.Models
{
    public class Driver
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم السائق مطلوب")]
        [StringLength(200)]
        [Display(Name = "اسم السائق")]
        public string Name { get; set; }

        [StringLength(30)]
        [Display(Name = "رقم الهاتف")]
        public string Phone { get; set; }

        [StringLength(50)]
        [Display(Name = "رقم السيارة")]
        public string CarNumber { get; set; }

        [Display(Name = "فعّال")]
        public bool IsActive { get; set; }
    }
}
