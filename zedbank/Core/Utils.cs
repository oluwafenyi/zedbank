namespace zedbank.Core;

public static class Utils
{
    public static DateTimeOffset GetStartOfDay(DateTimeOffset d)
    {
        return d.AddHours(-d.Hour).AddMinutes(-d.Minute).AddSeconds(-d.Second).AddMilliseconds(-d.Millisecond);
    }
}