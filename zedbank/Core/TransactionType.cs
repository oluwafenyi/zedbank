using System.ComponentModel;

namespace zedbank.Core;

public enum TransactionType
{
    [Description("Credit")]
    Credit = 1,
    [Description("Debit")]
    Debit = 2
}

public static class TransactionTypeExtensions
{
    public static string ToDescriptionString(this TransactionType val)
    {
        var attributes = val
            .GetType()
            .GetField(val.ToString())?
            .GetCustomAttributes(typeof(DescriptionAttribute), false);
        if (attributes == null)
        {
            return string.Empty;
        }
        var descAttributes = (DescriptionAttribute[])attributes;
        return descAttributes.Length > 0 ? descAttributes[0].Description : string.Empty;
    }
}
