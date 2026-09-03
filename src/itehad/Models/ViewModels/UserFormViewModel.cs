using System.ComponentModel.DataAnnotations;

namespace itehad.Models.ViewModels
{
    public class UserFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم المستخدم مطلوب")]
        [StringLength(50)]
        [Display(Name = "اسم المستخدم")]
        public string Username { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور")]
        public string Password { get; set; }

        [Required(ErrorMessage = "الاسم الظاهر مطلوب")]
        [StringLength(200)]
        [Display(Name = "الاسم الظاهر")]
        public string DisplayName { get; set; }

        [Display(Name = "مدير (صلاحية كاملة)")]
        public bool IsAdmin { get; set; }

        [Display(Name = "فعّال")]
        public bool IsActive { get; set; } = true;

        public string[] SelectedModules { get; set; }
    }
}
