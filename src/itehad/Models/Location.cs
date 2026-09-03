using System.ComponentModel.DataAnnotations;

namespace itehad.Models
{
    public class Location
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم الموقع مطلوب")]
        [StringLength(200)]
        [Display(Name = "الموقع")]
        public string Name { get; set; }

        /// <summary>
        /// المواقع المستوردة من النظام القديم بتنحط IsActive = 0 حتى ما تظهر
        /// بقائمة «من / إلى» عند تسجيل رحلة جديدة. المواقع الجديدة فعّالة تلقائياً.
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
