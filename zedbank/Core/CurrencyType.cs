using System.ComponentModel;

namespace zedbank.Core;

public enum CurrencyType
{
    [Description("USD")]
    Usd = 1,
    [Description("NGN")]
    Ngn = 2
}

public static class CurrencyTypeExtensions
{
    public static string ToDescriptionString(this CurrencyType val)
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
