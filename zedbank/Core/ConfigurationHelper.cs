namespace zedbank.Core;

public class ConfigurationHelper
{
    public static IConfiguration Config = null!;

    public static void Initialize(IConfiguration configuration)
    {
        Config = configuration;
    }

    public static void InitializeTestConfig()
    {
        Config = new ConfigurationManager();
        Config["Jwt:Issuer"] = "http://localhost:8000";
        Config["Jwt:Key"] = "zedbank-test-key";
    }
}