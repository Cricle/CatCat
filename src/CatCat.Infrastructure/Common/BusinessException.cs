namespace CatCat.Infrastructure.Common;

/// <summary>
/// Business exception for validation and business rule violations
/// </summary>
public class BusinessException : Exception
{
    public BusinessException(string message) : base(message)
    {
    }

    public BusinessException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}

