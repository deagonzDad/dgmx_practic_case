namespace api.Exceptions;

public class BaseApiException(string message, int errorCode) : Exception(message)
{
    public int ErrorCode { get; protected set; } = errorCode;
    public Exception? LogError { get; protected set; }
}

public class UnauthorizedActionException : BaseApiException
{
    private new const int ErrorCode = 401;
    private new const string Message = "Unauthorized action";

    public UnauthorizedActionException(Exception? logError)
        : base(Message, ErrorCode)
    {
        LogError = logError;
    }
}

public class UpdateException : BaseApiException
{
    private new const int ErrorCode = 400;
    private new const string Message = "Something went wrong with the update";

    public UpdateException(Exception logError)
        : base(Message, ErrorCode)
    {
        LogError = logError;
    }

    public UpdateException(Exception logError, string message)
        : base(message, ErrorCode)
    {
        LogError = logError;
    }
}
