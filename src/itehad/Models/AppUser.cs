using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace itehad.Models
{
    public class AppUser
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم المستخدم مطلوب")]
        [StringLength(50)]
        [Display(Name = "اسم المستخدم")]
        public string Username { get; set; }

        public string PasswordHash { get; set; }

        [Required(ErrorMessage = "الاسم الظاهر مطلوب")]
        [StringLength(200)]
        [Display(Name = "الاسم الظاهر")]
        public string DisplayName { get; set; }

        [Display(Name = "مدير")]
        public bool IsAdmin { get; set; }

        [Display(Name = "فعّال")]
        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }

        public virtual ICollection<AppUserModule> Modules { get; set; } = new List<AppUserModule>();
    }
}
