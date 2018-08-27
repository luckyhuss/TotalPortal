using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Web.Models;

namespace Web
{
    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your email service here to send an email.
            return Task.FromResult(0);
        }
    }

    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }

    public class ApplicationUserStore : UserStore<ApplicationUser, ApplicationRole, int, ApplicationUserLogin, ApplicationUserRole, ApplicationUserClaim>, IUserStore<ApplicationUser, int>
    {
        public ApplicationUserStore(ApplicationDbContext context)
            : base(context)
        {
        }
    }

    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
    public class ApplicationUserManager : UserManager<ApplicationUser, int>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser, int> store)
            : base(store)
        {
        }
        
        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context) 
        {
            var manager = new ApplicationUserManager(new ApplicationUserStore(context.Get<ApplicationDbContext>()));
            
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser, int>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug it in here.
            manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<ApplicationUser, int>
            {
                MessageFormat = "Your security code is {0}"
            });
            manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<ApplicationUser, int>
            {
                Subject = "Security Code",
                BodyFormat = "Your security code is {0}"
            });
            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = 
                    new DataProtectorTokenProvider<ApplicationUser, int>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }

        public static ApplicationUser GetUser(int _userId)
        {
            return GetUser(new ApplicationDbContext(), _userId);
        }

        public static ApplicationUser GetUser(ApplicationDbContext db, int _userId)
        {

            ApplicationUser _retVal = null;
            try
            {
                _retVal = db.Users.Where(p => p.Id == _userId).Include("Roles").Include(x => x.Roles.Select(r => r.Role.Permissions)).FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }

            return _retVal;
        }

        public static List<ApplicationUser> GetUsers()
        {
            List<ApplicationUser> _retVal = null;
            try
            {
                using (ApplicationDbContext db = new ApplicationDbContext())
                {
                    _retVal = db.Users.Where(r => r.Inactive == false || r.Inactive == null).OrderBy(r => r.Lastname).ThenBy(r => r.Firstname).ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }

            return _retVal;
        }

        public static List<ApplicationUser> GetUsers4Surname(string _surname)
        {
            List<ApplicationUser> _retVal = null;
            try
            {
                using (ApplicationDbContext db = new ApplicationDbContext())
                {
                    _retVal = db.Users.Where(r => r.Inactive == false || r.Inactive == null & r.Lastname == _surname).OrderBy(r => r.Lastname).ThenBy(r => r.Firstname).ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }

            return _retVal;
        }

        public static bool AddUser2Role(int _userId, int _roleId)
        {
            bool _retVal = false;
            try
            {
                using (ApplicationDbContext db = new ApplicationDbContext())
                {
                    ApplicationUser _user = GetUser(db, _userId);
                    if (_user.Roles.Where(p => p.RoleId == _roleId).Count() == 0)
                    {
                        //_user.UserRoles.Add(_role);

                        ApplicationUserRole _identityRole = new ApplicationUserRole { UserId = _userId, RoleId = _roleId };
                        if (!_user.Roles.Contains(_identityRole))
                            _user.Roles.Add(_identityRole);

                        _user.LastModified = DateTime.Now;
                        db.Entry(_user).State = EntityState.Modified;
                        db.SaveChanges();

                        _retVal = true;
                    }
                }
            }
            catch (Exception)
            {
            }
            return _retVal;
        }

        public static bool RemoveUser4Role(int _userId, int _roleId)
        {
            bool _retVal = false;
            try
            {
                using (ApplicationDbContext db = new ApplicationDbContext())
                {
                    ApplicationUser _user = GetUser(db, _userId);
                    if (_user.Roles.Where(p => p.RoleId == _roleId).Count() > 0)
                    {
                        _user.Roles.Remove(_user.Roles.Where(p => p.RoleId == _roleId).FirstOrDefault());
                        _user.LastModified = DateTime.Now;
                        db.Entry(_user).State = EntityState.Modified;
                        db.SaveChanges();

                        _retVal = true;
                    }
                }
            }
            catch (Exception)
            {
            }
            return _retVal;
        }

        public static bool DeleteUser(int _userId)
        {
            bool _retVal = false;
            try
            {
                using (ApplicationDbContext db = new ApplicationDbContext())
                {
                    //ApplicationUser _user = db.Users.Where(p => p.Id == _userId).Include("ROLES").FirstOrDefault();                 
                    ApplicationUser _user = GetUser(db, _userId);

                    _user.Roles.Clear();
                    db.Entry(_user).State = EntityState.Deleted;
                    db.SaveChanges();

                    _retVal = true;
                }
            }
            catch (Exception)
            {
            }
            return _retVal;
        }

        public static bool UpdateUser(UserViewModel _user)
        {
            bool _retVal = false;
            try
            {
                using (ApplicationDbContext db = new ApplicationDbContext())
                {
                    ApplicationUser _user2Modify = GetUser(db, _user.Id);

                    db.Entry(_user2Modify).Entity.UserName = _user.UserName;
                    db.Entry(_user2Modify).Entity.Email = _user.Email;
                    db.Entry(_user2Modify).Entity.Firstname = _user.Firstname;
                    db.Entry(_user2Modify).Entity.Lastname = _user.Lastname;
                    db.Entry(_user2Modify).Entity.LastModified = System.DateTime.Now;
                    db.Entry(_user2Modify).State = EntityState.Modified;
                    db.SaveChanges();

                    _retVal = true;
                }
            }
            catch (Exception ex)
            {
                string x = ex.Message;
            }
            return _retVal;
        }

        public static List<ApplicationUser> GetUsers4SelectList()
        {
            List<ApplicationUser> _retVal = null;
            try
            {
                using (ApplicationDbContext db = new ApplicationDbContext())
                {
                    _retVal = db.Users.Where(r => r.Inactive == false || r.Inactive == null).ToList();
                }
            }
            catch (Exception)
            {
            }

            return _retVal;
        }
    }

    // Configure the application sign-in manager which is used in this application.
    public class ApplicationSignInManager : SignInManager<ApplicationUser, int>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }

    public class ApplicationRoleStore : RoleStore<ApplicationRole, int, ApplicationUserRole>
    {
        public ApplicationRoleStore(ApplicationDbContext context)
            : base(context)
        {
        }
    }

    public class ApplicationRoleManager : RoleManager<ApplicationRole, int>
    {
        public ApplicationRoleManager(IRoleStore<ApplicationRole, int> store) : base(store)
        { }

        public static ApplicationRoleManager Create(IdentityFactoryOptions<ApplicationRoleManager> options, IOwinContext context)
        {
            return new ApplicationRoleManager(new RoleStore<ApplicationRole, int, ApplicationUserRole>(context.Get<ApplicationDbContext>()));
        }

        public static List<ApplicationRole> GetRoles()
        {
            List<ApplicationRole> _retVal = null;
            try
            {
                using (RoleStore<ApplicationRole, int, ApplicationUserRole> db = new RoleStore<ApplicationRole, int, ApplicationUserRole>(new ApplicationDbContext()))
                {
                    _retVal = db.Roles.Include("Permissions").ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return _retVal;
        }

        public static ApplicationRole GetRole(int _roleId)
        {
            ApplicationRole _retVal = null;
            try
            {
                using (RoleStore<ApplicationRole, int, ApplicationUserRole> db = new RoleStore<ApplicationRole, int, ApplicationUserRole>(new ApplicationDbContext()))
                {
                    _retVal = db.Roles.Where(p => p.Id == _roleId).Include("Permissions").FirstOrDefault();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return _retVal;
        }

        public static bool CreateRole(ApplicationRole _role)
        {
            bool _retVal = false;
            try
            {
                var roleManager = new RoleManager<ApplicationRole, int>(new RoleStore<ApplicationRole, int, ApplicationUserRole>(new ApplicationDbContext()));
                if (!roleManager.RoleExists(_role.Name))
                {
                    //_role.Id = Guid.NewGuid().ToString();
                    _role.LastModified = DateTime.Now;
                    roleManager.Create(_role);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return _retVal;
        }

        public static bool AddPermission2Role(int _roleId, int _PermissionId)
        {
            bool _retVal = false;
            try
            {
                using (ApplicationDbContext db = new ApplicationDbContext())
                {
                    ApplicationRole role = db.Roles.Where(p => p.Id == _roleId).Include("Permissions").FirstOrDefault();
                    if (role != null)
                    {
                        Permission _Permission = db.Permissions.Where(p => p.Id == _PermissionId).Include("Roles").FirstOrDefault();
                        if (!role.Permissions.Contains(_Permission))
                        {
                            role.Permissions.Add(_Permission);
                            role.LastModified = DateTime.Now;
                            db.Entry(role).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return _retVal;
        }

        public static bool AddAllPermissions2Role(int _roleId)
        {
            bool _retVal = false;
            try
            {
                using (ApplicationDbContext db = new ApplicationDbContext())
                {
                    ApplicationRole role = db.Roles.Where(p => p.Id == _roleId).Include("Permissions").FirstOrDefault();
                    if (role != null)
                    {
                        List<Permission> _Permissions = db.Permissions.Include("ROLES").ToList();
                        foreach (Permission _Permission in _Permissions)
                        {
                            if (!role.Permissions.Contains(_Permission))
                            {
                                role.Permissions.Add(_Permission);
                            }
                        }
                        role.LastModified = DateTime.Now;
                        db.Entry(role).State = EntityState.Modified;
                        db.SaveChanges();
                        _retVal = true;
                    }
                }
            }
            catch
            {
            }
            return _retVal;
        }

        public static bool UpdateRole(RoleViewModel _modifiedRole)
        {
            bool _retVal = false;
            try
            {
                using (ApplicationDbContext db = new ApplicationDbContext())
                {
                    ApplicationRole _role2Modify = db.Roles.Where(p => p.Id == _modifiedRole.Id).Include("Permissions").FirstOrDefault();

                    db.Entry(_role2Modify).Entity.Name = _modifiedRole.Name;
                    db.Entry(_role2Modify).Entity.Description = _modifiedRole.RoleDescription;
                    db.Entry(_role2Modify).Entity.IsSysAdmin = _modifiedRole.IsSysAdmin;
                    db.Entry(_role2Modify).Entity.LastModified = System.DateTime.Now;
                    db.Entry(_role2Modify).State = EntityState.Modified;
                    db.SaveChanges();

                    _retVal = true;
                }
            }
            catch (Exception)
            {
            }
            return _retVal;
        }

        public static bool DeleteRole(int _roleId)
        {
            bool _retVal = false;
            try
            {
                using (ApplicationDbContext db = new ApplicationDbContext())
                {
                    ApplicationRole _role2Delete = db.Roles.Where(p => p.Id == _roleId).Include("Permissions").FirstOrDefault();
                    if (_role2Delete != null)
                    {
                        _role2Delete.Permissions.Clear();
                        db.Entry(_role2Delete).State = EntityState.Deleted;
                        db.SaveChanges();
                        _retVal = true;
                    }
                }
            }
            catch (Exception)
            {
            }
            return _retVal;
        }

        public static bool RemovePermission4Role(int _roleId, int _PermissionId)
        {
            bool _retVal = false;
            try
            {
                using (ApplicationDbContext db = new ApplicationDbContext())
                {
                    ApplicationRole _role2Modify = db.Roles.Where(p => p.Id == _roleId).Include("Permissions").FirstOrDefault();
                    Permission _Permission = db.Permissions.Where(p => p.Id == _PermissionId).Include("Roles").FirstOrDefault();

                    if (_role2Modify.Permissions.Contains(_Permission))
                    {
                        _role2Modify.Permissions.Remove(_Permission);
                        _role2Modify.LastModified = DateTime.Now;
                        db.Entry(_role2Modify).State = EntityState.Modified;
                        db.SaveChanges();

                        _retVal = true;
                    }
                }
            }
            catch (Exception)
            {
            }
            return _retVal;
        }

        public static List<ApplicationRole> GetRoles4SelectList()
        {
            List<ApplicationRole> _retVal = null;
            try
            {
                using (ApplicationDbContext db = new ApplicationDbContext())
                {
                    _retVal = db.Roles.OrderBy(p => p.Name).ToList();
                }
            }
            catch (Exception)
            {
            }

            return _retVal;
        }

        public static List<Permission> GetPermissions4SelectList()
        {
            List<Permission> _retVal = null;
            try
            {
                using (ApplicationDbContext db = new ApplicationDbContext())
                {
                    _retVal = db.Permissions.OrderBy(p => p.Description).ToList();
                }
            }
            catch (Exception)
            {
            }

            return _retVal;
        }

        #region Worker functions for Permissions
        public static List<Permission> GetPermissions()
        {
            List<Permission> _retVal = null;
            try
            {
                using (ApplicationDbContext db = new ApplicationDbContext())
                {
                    _retVal = db.Permissions.OrderBy(p => p.Description).Include("Roles").ToList();
                }
            }
            catch (Exception)
            {
            }
            return _retVal;
        }

        public static Permission GetPermission(int _Permissionid)
        {
            Permission _retVal = null;
            try
            {
                using (ApplicationDbContext db = new ApplicationDbContext())
                {
                    _retVal = db.Permissions.Where(p => p.Id == _Permissionid).Include("Roles").FirstOrDefault();
                }
            }
            catch (Exception)
            {
            }
            return _retVal;
        }

        /*public static Permission GetPermission4Description(string _permDescription)
        {
            Permission _retVal = null;
            try
            {
                using (ApplicationDbContext db = new ApplicationDbContext())
                {
                    _retVal = db.Permissions.Where(p => p.PermissionDescription == _permDescription).Include("ROLES").FirstOrDefault();
                }
            }
            catch (Exception)
            {
            }
            return _retVal;
        }*/


        public static bool AddPermission(Permission _newPermission)
        {
            bool _retVal = false;
            try
            {
                using (ApplicationDbContext db = new ApplicationDbContext())
                {
                    db.Permissions.Add(_newPermission);
                    db.Entry(_newPermission).State = EntityState.Added;
                    db.SaveChanges();
                    _retVal = true;
                }
            }
            catch (Exception)
            {
            }
            return _retVal;
        }

        public static bool UpdatePermission(Permission _Permission)
        {
            bool _retVal = false;
            try
            {
                using (ApplicationDbContext db = new ApplicationDbContext())
                {
                    db.Entry(_Permission).State = EntityState.Modified;
                    db.SaveChanges();
                    _retVal = true;
                }
            }
            catch (Exception)
            {
            }
            return _retVal;
        }

        public static bool DeletePermission(int _PermissionId)
        {
            bool _retVal = false;
            try
            {
                using (ApplicationDbContext db = new ApplicationDbContext())
                {
                    Permission _Permission = db.Permissions.Where(p => p.Id == _PermissionId).Include("Roles").FirstOrDefault();

                    _Permission.Roles.Clear();
                    db.Entry(_Permission).State = EntityState.Deleted;
                    db.SaveChanges();
                    _retVal = true;
                }
            }
            catch (Exception)
            {
            }
            return _retVal;
        }
        #endregion
    }


}
