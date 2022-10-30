using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using zedbank.Core;

namespace zedbank.Models;

[Table("transactions")]
public class Transaction
{
    public long Id { get; set; }

    public string Reference { get; init; } = null!;
    
    public TransactionClassification Classification { get; set; }
    public TransactionType Type { get; set; }
    public TransactionStatus Status { get; set; }
    
    [Precision(19, 4)]
    public decimal HistoricalBalance { get; set; }
    
    [Precision(19, 4)]
    public decimal Amount { get; set; }
    
    public DateTimeOffset Created { get; set; }
    public long WalletId { get; set; }
    
    public Wallet? Wallet { get; set; }

    public static string GenerateReference()
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var stringChars = new char[8];
        var random = new Random();

        for (int i = 0; i < stringChars.Length; i++)
        {
            stringChars[i] = chars[random.Next(chars.Length)];
        }

        return "Z-" + new String(stringChars);
    }
}

public class TransactionDto
{
    public long Id { get; set; }
    public string Reference { get; set; }
    public string Classification { get; set; }
    public string Type { get; set; }
    public string Status { get; set; }
    public string HistoricalBalance { get; set; }
    public string Amount { get; set; }
    public DateTimeOffset Created { get; set; }

    public TransactionDto(Transaction transaction)
    {
        Id = transaction.Id;
        Reference = transaction.Reference;
        Classification = transaction.Classification.ToDescriptionString();
        Type = transaction.Type.ToString();
        Status = transaction.Status.ToString();
        HistoricalBalance = transaction.HistoricalBalance.ToString("0.00");
        Amount = transaction.Amount.ToString("0.00");
        Created = transaction.Created;
    }
}
