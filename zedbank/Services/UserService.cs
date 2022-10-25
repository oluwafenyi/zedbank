using Microsoft.EntityFrameworkCore;
using zedbank.Core;
using zedbank.Database;
using zedbank.Exceptions;
using zedbank.Models;

namespace zedbank.Services;

public static class UserService
{
    public static async Task<User> RegisterUser(UserRegistrationDto data, Context context)
    {
        User user;
        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            user = new User(data.Email, data.FirstName, data.LastName);
            user.SetPassword(data.Password);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var nairaWallet = new Wallet(CurrencyType.Ngn, user.Id);
            var usdWallet = new Wallet(CurrencyType.Usd, user.Id);
            context.Wallets.Add(nairaWallet);
            context.Wallets.Add(usdWallet);
            await context.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw new UserRegistrationException(e);
        }
        return user;
    }
}