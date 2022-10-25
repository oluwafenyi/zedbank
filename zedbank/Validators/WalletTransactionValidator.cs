using FluentValidation;
using zedbank.Models;

namespace zedbank.Validators;

public class WalletTransactionValidator: AbstractValidator<WalletTransactionDto>
{
    public WalletTransactionValidator()
    {
        RuleFor(t => t.Amount).NotEmpty().NotNull().Must(ValidCurrencyValue);
    }

    private bool ValidCurrencyValue(string? amount)
    {
        if (amount == null)
        {
            return false;
        }
        try
        {
            var amnt = decimal.Parse(amount);
            return decimal.Round(amnt, 2) == amnt && amnt > 0;
        }
        catch (Exception)
        {
            return false;
        }
    }
}