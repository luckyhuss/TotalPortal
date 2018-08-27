using System;
using System.Collections.Generic;
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
    public class ApplicationUser : IdentityUser<int, ApplicationUserLogin, ApplicationUserRole, ApplicationUserClaim>
    {
        public DateTime LastModified { get; set; }

        public bool Inactive { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }

        public ApplicationUser()
        {
            LastModified = DateTime.Now;
            Inactive = false;
        }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser, int> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }

        public bool IsPermissionInUserRoles(string _permission)
        {
            bool _retVal = false;
            try
            {
                foreach (ApplicationUserRole _role in this.Roles)
                {
                    if (_role.IsPermissionInRole(_permission))
                    {
                        _retVal = true;
                        break;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return _retVal;
        }

        public bool IsSysAdmin()
        {
            bool _retVal = false;
            try
            {
                foreach (ApplicationUserRole _role in this.Roles)
                {
                    if (_role.IsSysAdmin)
                    {
                        _retVal = true;
                        break;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return _retVal;
        }
    }

    /// <summary>
    /// Extended IdentityUserLogin class with an integer as key to indicate that the 'UserId' will be an int instead of the default string
    /// </summary>
    public class ApplicationUserLogin : IdentityUserLogin<int>
    {

    }

    /// <summary>
    /// Extended IdentityRole class with an integer as key to indicate that the role 'Id' will be an int instead of the default string
    /// </summary>
    public class ApplicationRole : IdentityRole<int, ApplicationUserRole>
    {
        public ApplicationRole()
        {
            //this.Id = Guid.NewGuid().ToString();
        }

        public ApplicationRole(string name)
            : this()
        {
            this.Name = name;
        }

        public ApplicationRole(string name, string description)
            : this(name)
        {
            this.Description = description;
        }

        public DateTime LastModified { get; set; }
        public string Description { get; set; }
        public bool IsSysAdmin { get; set; }

        public virtual ICollection<Permission> Permissions { get; set; }

        public bool IsPermissionInRole(string _permission)
        {
            bool _retVal = false;
            try
            {
                foreach (Permission _perm in this.Permissions)
                {
                    if (_perm.Description == _permission)
                    {
                        _retVal = true;
                        break;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return _retVal;
        }
    }

    /// <summary>
    /// Extended IdentityUserRole class with an integer as key to indicate that the 'UserId' and 'RoleId' will be an int instead of the default string
    /// </summary>
    public class ApplicationUserRole : IdentityUserRole<int>
    {

        public ApplicationUserRole() : base()
        { }

        public ApplicationRole Role { get; set; }

        public bool IsPermissionInRole(string _permission)
        {
            bool _retVal = false;
            try
            {
                _retVal = this.Role.IsPermissionInRole(_permission);
            }
            catch (Exception)
            {
            }
            return _retVal;
        }

        public bool IsSysAdmin { get { return this.Role.IsSysAdmin; } }
    }

    /// <summary>
    /// Extended IdentityUserClaim class with an integer as key to indicate that the 'UserId' will be an int instead of the default string
    /// </summary>
    public class ApplicationUserClaim : IdentityUserClaim<int>
    {

    }

    public partial class Permission
    {
        //public Permission()
        //{
        //    this.Roles = new HashSet<ApplicationRole>();
        //}

        public int Id { get; set; }
        public string Description { get; set; }

        public virtual ICollection<ApplicationRole> Roles { get; set; }
    }

    /// <summary>
    /// Extended IdentityDbContext class for authentication and authorization via Identity framework 
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int, ApplicationUserLogin, ApplicationUserRole, ApplicationUserClaim>
    {
        public DbSet<Permission> Permissions { get; set; }
         
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
            modelBuilder.Entity<ApplicationUser>().ToTable("Users").Property(p => p.Id).HasColumnName("Id");
            modelBuilder.Entity<ApplicationRole>().ToTable("Roles").Property(p => p.Id).HasColumnName("Id");
            modelBuilder.Entity<ApplicationUserRole>().ToTable("UserRoles");
            modelBuilder.Entity<ApplicationUserLogin>().ToTable("UserLogins");
            modelBuilder.Entity<ApplicationUserClaim>().ToTable("UserClaims");

            modelBuilder.Entity<ApplicationRole>().
            HasMany(c => c.Permissions).
            WithMany(p => p.Roles).
            Map(
                m =>
                {
                    m.MapLeftKey("RoleId");
                    m.MapRightKey("PermissionId");
                    m.ToTable("RolePermissions");
                });
        }
    }
}