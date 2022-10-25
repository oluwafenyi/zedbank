using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using zedbank.Core;

namespace zedbank.Models;

[Table("wallets")]
public class Wallet
{
    public long Id { get; set; }
    
    [Precision(19, 4)]
    public decimal Balance { get; set; }
    
    public CurrencyType Currency { get; set; }
    public long OwnerId { get; set; }
    
    [Required]
    public User? Owner { get; set; }

    public List<Transaction> Transactions { get; } = new();

    public Wallet(CurrencyType currency, long ownerId)
    {
        Balance = 0;
        Currency = currency;
        OwnerId = ownerId;
    }
}

public class UserWalletDisplayDto
{
    public long Id { get; }
    public string Balance { get; }
    public string CurrencyType { get; }

    public UserWalletDisplayDto(Wallet wallet)
    {
        Id = wallet.Id;
        Balance = wallet.Balance.ToString("0.00");
        CurrencyType = wallet.Currency.ToDescriptionString();
    }
}

public class WalletTransactionDto
{
    public string Amount { get; set; }

    public WalletTransactionDto(string amount)
    {
        Amount = amount;
    }
}