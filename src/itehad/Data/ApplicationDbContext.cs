using System.Data.Entity;
using itehad.Models;

namespace itehad.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() : base("name=ApplicationDbContext")
        {
        }

        public DbSet<BookingSource> BookingSources { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<TripDriver> TripDrivers { get; set; }
        public DbSet<DriverAttendance> DriverAttendances { get; set; }
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<AppUserModule> AppUserModules { get; set; }
        public DbSet<AppSetting> AppSettings { get; set; }
        public DbSet<CustomerPayment> CustomerPayments { get; set; }
        public DbSet<ExpenseCategory> ExpenseCategories { get; set; }
        public DbSet<Expense> Expenses { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BookingSource>().ToTable("BookingSources");
            modelBuilder.Entity<Location>().ToTable("Locations");
            modelBuilder.Entity<Customer>().ToTable("Customers");
            modelBuilder.Entity<Driver>().ToTable("Drivers");
            modelBuilder.Entity<Trip>().ToTable("Trips");
            modelBuilder.Entity<TripDriver>().ToTable("TripDrivers");
            modelBuilder.Entity<DriverAttendance>().ToTable("DriverAttendance");
            modelBuilder.Entity<AppUser>().ToTable("AppUsers");
            modelBuilder.Entity<AppUserModule>().ToTable("AppUserModules");
            modelBuilder.Entity<AppSetting>().ToTable("AppSettings");
            modelBuilder.Entity<CustomerPayment>().ToTable("CustomerPayments");
            modelBuilder.Entity<ExpenseCategory>().ToTable("ExpenseCategories");
            modelBuilder.Entity<Expense>().ToTable("Expenses");

            modelBuilder.Entity<Trip>()
                .HasRequired(t => t.BookingSource)
                .WithMany()
                .HasForeignKey(t => t.BookingSourceId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Trip>()
                .HasRequired(t => t.Customer)
                .WithMany()
                .HasForeignKey(t => t.CustomerId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Trip>()
                .HasRequired(t => t.FromLocation)
                .WithMany()
                .HasForeignKey(t => t.FromLocationId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Trip>()
                .HasRequired(t => t.ToLocation)
                .WithMany()
                .HasForeignKey(t => t.ToLocationId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TripDriver>()
                .HasKey(td => new { td.TripId, td.DriverId });

            modelBuilder.Entity<TripDriver>()
                .HasRequired(td => td.Trip)
                .WithMany(t => t.TripDrivers)
                .HasForeignKey(td => td.TripId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<TripDriver>()
                .HasRequired(td => td.Driver)
                .WithMany()
                .HasForeignKey(td => td.DriverId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DriverAttendance>()
                .HasRequired(a => a.Driver)
                .WithMany()
                .HasForeignKey(a => a.DriverId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<AppUserModule>()
                .HasKey(m => new { m.UserId, m.ModuleKey });

            modelBuilder.Entity<AppUserModule>()
                .HasRequired(m => m.User)
                .WithMany(u => u.Modules)
                .HasForeignKey(m => m.UserId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<CustomerPayment>()
                .HasRequired(p => p.Customer)
                .WithMany()
                .HasForeignKey(p => p.CustomerId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Expense>()
                .HasRequired(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Expense>()
                .HasOptional(e => e.Driver)
                .WithMany()
                .HasForeignKey(e => e.DriverId)
                .WillCascadeOnDelete(false);
        }
    }
}
