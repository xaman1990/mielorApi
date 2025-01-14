using Microsoft.EntityFrameworkCore;
using AlphaApi.Models;
namespace AlphaApi.Data
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

      
        public DbSet<ApiKey> ApiKeys { get; set; }

        public DbSet<ConfiguracionToken> ConfiguracionToken { get; set; }

        public DbSet<Tokens> Tokens { get; set; }

        public DbSet<RegisterTokenRequest> RegisterTokenRequest { get; set; }   
    }
}
