using System.Transactions;
using Microsoft.EntityFrameworkCore;
using zedbank.Core;
using zedbank.Database;
using zedbank.Models;
using IsolationLevel = System.Data.IsolationLevel;
using Transaction = zedbank.Models.Transaction;
using TransactionStatus = zedbank.Core.TransactionStatus;

namespace zedbank.Services;

public static class WalletService
{
    public static bool TestMode = false;
    
    public static async Task<Wallet?> GetWallet(long id, Context context)
    {
        return await context.Wallets.FindAsync(id);
    }

    public static async Task FundWallet(Wallet wallet, WalletTransactionDto data, Context context)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        try
        {
            if (!TestMode)
            {
                await context.Database.ExecuteSqlRawAsync(
                    $"SELECT 1 FROM wallets WITH (UPDLOCK) WHERE Id = {wallet.Id}");  
            }
            var w = await context.Wallets.FindAsync(wallet.Id);
            if (w == null)
            {
                throw new Exception();
            }

            var oldBalance = w.Balance;
            var amount = decimal.Parse(data.Amount);
            w.Balance = w.Balance + amount;
            await context.SaveChangesAsync();

            var t = new Transaction
            {
                Reference = Transaction.GenerateReference(),
                Description = "fund",
                Type = TransactionType.Credit,
                Status = TransactionStatus.Successful,
                HistoricalBalance = oldBalance,
                Amount = amount,
                WalletId = wallet.Id,
            };
            context.Transactions.Add(t);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw new TransactionException();
        }
    }
    
    public static async Task WithdrawFromWallet(Wallet wallet, WalletTransactionDto data, Context context)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        try
        {
            if (!TestMode)
            {
                await context.Database.ExecuteSqlRawAsync(
                    $"SELECT 1 FROM wallets WITH (UPDLOCK) WHERE Id = {wallet.Id}");
            }
            var w = await context.Wallets.FindAsync(wallet.Id);
            if (w == null)
            {
                throw new Exception();
            }
            
            var oldBalance = w.Balance;
            var amount = decimal.Parse(data.Amount);
            if (amount > oldBalance)
            {
                throw new Exceptions.TransactionOverdraftException();
            }
            
            w.Balance = w.Balance - amount;
            await context.SaveChangesAsync();

            var t = new Transaction
            {
                Reference = Transaction.GenerateReference(),
                Description = "withdraw",
                Type = TransactionType.Debit,
                Status = TransactionStatus.Successful,
                HistoricalBalance = oldBalance,
                Amount = amount,
                WalletId = wallet.Id,
            };
            context.Transactions.Add(t);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception e) when (e is not Exceptions.TransactionOverdraftException)
        {
            await transaction.RollbackAsync();
            throw new Exceptions.TransactionException(e);
        }
    }

    public static async Task<List<Transaction>> GetWalletTransactions(Wallet wallet, Context context)
    {
        var transactions = await context.Transactions.
            Where(t => t.WalletId == wallet.Id).
            OrderBy(t => t.Created).
            ToListAsync();
        return transactions;
    }
}