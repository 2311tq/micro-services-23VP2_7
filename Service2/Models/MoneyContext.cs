using Microsoft.EntityFrameworkCore;
using Service.Models;

namespace WebAPIApp.Models
{
    public class MoneyContext : DbContext
    {
        public DbSet<Money> Users { get; set; }
        public MoneyContext(DbContextOptions<MoneyContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }
    }
}