namespace api.Exceptions;

public class BaseApiException(string message, int errorCode) : Exception(message)
{
    public int ErrorCode { get; protected set; } = errorCode;
}

public class UnauthorizedActionException() : BaseApiException(Message, ErrorCode)
{
    private new const int ErrorCode = 401;
    private new const string Message = "Unauthorized action";
}
