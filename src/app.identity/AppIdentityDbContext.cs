using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace app.identity
{
    public class AppIdentityDbContext : IdentityDbContext<ApplicationUser>
    {
        private string schemaName = "identity";

        public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

            // Create objects in 'identity' schema instead of the default 'public' schema
            builder.HasDefaultSchema(schemaName);

        }
    }
}
