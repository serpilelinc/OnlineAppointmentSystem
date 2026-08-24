using AppointmentApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace AppointmentApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Appointment> Appointments { get; set; }

        public DbSet<AppointmentServiceType> ServiceTypes { get; set; }

        public DbSet<Staff> Staffs { get; set; }

        public DbSet<StaffServiceType> StaffServiceTypes { get; set; }

        public DbSet<StaffWorkingHour> StaffWorkingHours { get; set; }

        public DbSet<User> Users { get; set; }

        // Kategoriler
        public DbSet<Category> Categories { get; set; }

        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Appointment>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<AppointmentServiceType>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Category>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Staff>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<StaffServiceType>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<StaffWorkingHour>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Review>().HasQueryFilter(e => !e.IsDeleted);
        }

        public override int SaveChanges()
        {
            HandleSoftDelete();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            HandleSoftDelete();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void HandleSoftDelete()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Deleted && e.Entity is ISoftDeletable);

            foreach (var entry in entries)
            {
                entry.State = EntityState.Modified;
                ((ISoftDeletable)entry.Entity).IsDeleted = true;
            }
        }
    }
}