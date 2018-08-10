using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Web.Models
{
    /// <summary>
    /// Extended IdentityUser class to ovverride certain default properties
    /// You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    /// </summary>
    public class ApplicationUser : IdentityUser<int, CustomUserLogin, CustomUserRole, CustomUserClaim>
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser, int> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    /// <summary>
    /// Extended IdentityUserLogin class with an integer as key to indicate that the 'UserId' will be an int instead of the default string
    /// </summary>
    public class CustomUserLogin : IdentityUserLogin<int>
    {

    }

    /// <summary>
    /// Extended IdentityRole class with an integer as key to indicate that the role 'Id' will be an int instead of the default string
    /// </summary>
    public class CustomRole : IdentityRole<int, CustomUserRole>
    {

    }

    /// <summary>
    /// Extended IdentityUserRole class with an integer as key to indicate that the 'UserId' and 'RoleId' will be an int instead of the default string
    /// </summary>
    public class CustomUserRole : IdentityUserRole<int>
    {

    }

    /// <summary>
    /// Extended IdentityUserClaim class with an integer as key to indicate that the 'UserId' will be an int instead of the default string
    /// </summary>
    public class CustomUserClaim : IdentityUserClaim<int>
    {

    }

    /// <summary>
    /// Extended IdentityDbContext class for authentication and authorization via Identity framework 
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, CustomRole, int, CustomUserLogin, CustomUserRole, CustomUserClaim>
    {
        public ApplicationDbContext()
            : base("IdentityContext")
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // mapping of the extended Identity objects to their related tables in the database
            modelBuilder.Entity<ApplicationUser>().ToTable("Users");
            modelBuilder.Entity<CustomRole>().ToTable("Roles");
            modelBuilder.Entity<CustomUserRole>().ToTable("UserRoles");
            modelBuilder.Entity<CustomUserLogin>().ToTable("UserLogins");
            modelBuilder.Entity<CustomUserClaim>().ToTable("UserClaims");
        }
    }
}