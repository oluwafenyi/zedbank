using System.ComponentModel;

namespace zedbank.Core;

public enum TransactionStatus
{
    [Description("Pending")]
    Pending = 1,
    [Description("Successful")]
    Successful = 2,
    [Description("Failed")]
    Failed = 3
}

public static class TransactionStatusExtensions
{
    public static string ToDescriptionString(this TransactionStatus val)
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
