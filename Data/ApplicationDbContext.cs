using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace MyIdentityApp.Data {

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<CompanyEntity> Companies { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }

    public class CompanyEntity
    {
        public int Id { get; set; }
        List<ApplicationUser>? Users { get; set; }
        public string? CompanyName { get; set; }
    }

    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? CompanyName { get; set; }
        public List<DateTime>? LeaveDay { get; set; }
    }



    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
    }


}