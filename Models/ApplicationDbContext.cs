using MentalHealth_BackEnd.helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MentalHealth_BackEnd.Models
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
       : base(options)
        {
        }

        public DbSet<Post> posts { get; set; }
        public DbSet<Visitor> visitors { get; set; }
        public DbSet<TherapistAndDoctor> TherapistsAndDoctors { get; set; }
        public DbSet<Specialization> Specializations { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<ReactOnPost> ReactsOnPosts { get; set; }
        public DbSet<CommentOnPost> CommentsOnPosts { get; set; }
        public DbSet<Certificate> certificates { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<IdentityRole>().HasData(new IdentityRole
            {
                Id = "1",
                Name = "Visitor",
                NormalizedName = "VISITOR"
            });
            builder.Entity<IdentityRole>().HasData(new IdentityRole
            {
                Id = "2",
                Name = "Therapist",
                NormalizedName = "THERAPIST"
            });
            builder.Entity<IdentityRole>().HasData(new IdentityRole
            {
                Id = "3",
                Name = "Doctor",
                NormalizedName = "DOCTOR"
            });
            builder.Entity<IdentityRole>().HasData(new IdentityRole
            {
                Id = "4",
                Name = "Admin",
                NormalizedName = "ADMIN"
            });

            builder.Entity<CommentOnPost>()
                .HasOne(c => c.Post)
                .WithMany(p => p.comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ReactOnPost>()
                .HasOne(r => r.Post)
                .WithMany(p => p.reacts)
                .HasForeignKey(r => r.PostId)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(builder);
        }


        public async Task SeedAdminUser(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string adminEmail = "aymanzeinab55@gmail.com";
            string adminPassword = "Admin@123";
            string roleName = "Admin";

            if (!await roleManager.RoleExistsAsync(roleName))
                await roleManager.CreateAsync(new IdentityRole(roleName));

            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new User
                {
                    Name = "Admin",
                    UserName = "admin_aymanzeinab55",
                    Email = adminEmail,
                    Address = "Qena",
                    pathProfilePicture = "/images/default.jpg",
                    BornDate = new DateTime(1990, 1, 1),
                    gender = Gender.Male,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, roleName);
                }
            }
        }


    }
}
