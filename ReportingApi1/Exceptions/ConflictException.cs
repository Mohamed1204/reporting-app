namespace ReportingApi1.Exceptions;

public class ConflictException : AppException
{
    public ConflictException(string message) : base(message) { }
}
