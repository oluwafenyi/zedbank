using Microsoft.EntityFrameworkCore;
using zedbank.Models;

namespace zedbank.Database;

public class Context: DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    public Context(DbContextOptions<Context> opts) : base(opts) {}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var users = modelBuilder.Entity<User>().Property("_password");
        var transactions = modelBuilder.Entity<Transaction>();
        transactions.Property(t => t.Created).HasDefaultValueSql("getdate()");
    }
}