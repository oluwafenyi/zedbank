using System.Security.Claims;

namespace zedbank.Test;

public class TestPrincipal: ClaimsPrincipal
{
    public TestPrincipal(params Claim[] claims) : base(new TestIdentity(claims))
    {
        
    }
}

public class TestIdentity : ClaimsIdentity
{
    public TestIdentity(params Claim[] claims) : base(claims)
    {
        
    }
}