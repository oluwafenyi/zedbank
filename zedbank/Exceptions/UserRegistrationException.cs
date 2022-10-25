namespace zedbank.Exceptions;

public class UserRegistrationException: Exception
{
    public UserRegistrationException(Exception inner) : base("", inner)
    {
        
    }
} 