using Microsoft.EntityFrameworkCore;
using zedbank.Database;
using zedbank.Models;
using zedbank.Services;

namespace zedbankDataSeeder;

public class Program
{
    // creates 500k users and 1m wallets
    static async Task Main(string[] args)
    {
        var opts = new DbContextOptionsBuilder<Context>()
            .UseSqlServer("Data Source=localhost; Initial Catalog=zedbank; User Id=sa; Password=h8t7-X9IAO; TrustServerCertificate=true").Options;
        var ctx = new Context(opts);
        
        for (int i = 0; i < 500_000; i++)
        {
            await UserService.RegisterUser(new UserRegistrationDto()
            {
                Email = Faker.Internet.Email() + i.ToString(),
                FirstName = Faker.Name.First(),
                LastName = Faker.Name.Last(),
                Password = "password",
                ConfirmPassword = "password",
            }, ctx);
        }

        var wallets = ctx.Wallets.ToList();
        foreach (var wallet in wallets)
        {
            await WalletService.FundWallet(wallet, new WalletTransactionDto("15000.00"), ctx);
        }
    }
}