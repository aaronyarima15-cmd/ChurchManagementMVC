using ChurchManagementMVC.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChurchManagementMVC.Data
{
    public class AppDbContext : IdentityDbContext<Admins>
    {
        public DbSet<ChurchProfile> ChurchProfiles { get; set; } = null!;

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ChurchProfile>().HasData(new ChurchProfile
            {
                Id = 1,
                Name = "ECWA GIGINYU HAUSA SECTION",
                Address = "Enter church address here",
                Email = "Enter church email here",
                Phone = "Enter church phone here"
            });
        }
    }
}