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
        public DbSet<CVInfo> CVInfos { get; set; }
    }

    public class CompanyEntity
    {
        public int Id { get; set; }
        List<ApplicationUser>? Users { get; set; }
        public string? CompanyName { get; set; }
    }

/*     public class CVInfo
    {
        public int Id { get; set; }
        public string? Key { get; set; }
        public string? Value { get; set; }

        // Foreign key
        public string? ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }
    } */

    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Unit { get; set; }
        public string? CompanyName { get; set; }
        public List<DateTime>? LeaveDay { get; set; }
        public string? Department { get; set; }
        public ICollection<CVInfo>? CVInfos { get; set; }
        
    }



    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
    }


}