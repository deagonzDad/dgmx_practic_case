namespace api.Exceptions;

public class BaseApiException(string message, int errorCode) : Exception(message)
{
    public int ErrorCode { get; protected set; } = errorCode;
}
