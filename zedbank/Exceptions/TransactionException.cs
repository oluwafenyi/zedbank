namespace zedbank.Exceptions;

public class TransactionException: Exception
{
    public TransactionException(Exception inner) : base("", inner)
    {
        
    }
}

public class TransactionOverdraftException : Exception
{
    public TransactionOverdraftException() : base("")
    {
        
    }
}