using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using clsCms.Models; 

namespace clsCMs.Data
{
    public class ApplicationDbContext: IdentityDbContext<UserModel>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Organizations
        public DbSet<OrganizationModel> Organizations { get; set; }
        public DbSet<OrganizationMemberModel> Memberships { get; set; }

        // Channels
        public DbSet<ChannelModel> Channels { get; set; }
        public DbSet<ChannelMembershipModel> ChannelMemberships { get; set; }

        // Analytics
        public DbSet<AdImpressionModel> AdImpressions { get; set; }
        public DbSet<AdClickModel> AdClicks { get; set; }
        public DbSet<ArticleImpressionModel> ArticleImpressions { get; set; }
    }
}