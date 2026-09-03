using System.ComponentModel.DataAnnotations;

namespace itehad.Models
{
    public class ExpenseCategory
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم التصنيف مطلوب")]
        [StringLength(100)]
        [Display(Name = "تصنيف المصاريف")]
        public string Name { get; set; }
    }
}
