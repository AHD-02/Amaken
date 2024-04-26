using Amaken.Types;
using Microsoft.EntityFrameworkCore;

namespace Amaken.Models
{
    public class ApplicationDbContext: DbContext
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Server=db-postgresql-fra1-83565-do-user-16067379-0.c.db.ondigitalocean.com;Port=25060;Database=defaultdb;User Id=doadmin;Password=AVNS__SsPpnZREcQbf_65XL0;Trust Server Certificate=true;");
        }

        public DbSet<User> User { get; set; }
        public DbSet<Admin> Admin { get; set; }
        public DbSet<Event> Event { get; set; }
        public DbSet<Owner> Owner { get; set; }
        public DbSet<Private_Place> Private_Place { get; set; }
        public DbSet<Public_Place> Public_Place { get; set; }
        public DbSet<Reservation> Reservation { get; set; }
        public DbSet<CommonTypes.City> City { get; set; }
    }
}
