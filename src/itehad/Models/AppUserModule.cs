namespace itehad.Models
{
    public class AppUserModule
    {
        public int UserId { get; set; }
        public virtual AppUser User { get; set; }

        public string ModuleKey { get; set; }
    }
}
