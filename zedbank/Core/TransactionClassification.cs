using System.ComponentModel;

namespace zedbank.Core;

public enum TransactionClassification
{
    [Description("Funding")]
    Funding = 1,
    [Description("Withdrawal")]
    Withdrawal = 2,
    [Description("Reversal")]
    Reversal = 3,
    [Description("SimpleInterestCredit")]
    SimpleInterestCredit = 4,
}

public static class TransactionClassificationExtensions
{
    public static string ToDescriptionString(this TransactionClassification val)
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
