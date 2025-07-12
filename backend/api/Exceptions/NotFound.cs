namespace api.Exceptions;

public class UserNotFoundException : BaseApiException
{
    private const int NotFoundCode = 404;
    private const string DefaultMessage = "User not found";

    public UserNotFoundException(Exception? logError)
        : base(DefaultMessage, NotFoundCode)
    {
        LogError = logError;
    }

    public UserNotFoundException(Exception logError, string message)
        : base(message, NotFoundCode)
    {
        LogError = logError;
    }
}

public class RoomNotFoundException : BaseApiException
{
    private const string DefaultMessage = "Room not found";
    private const int NotFoundCode = 404;

    public RoomNotFoundException(Exception? logError)
        : base(DefaultMessage, NotFoundCode)
    {
        LogError = logError;
    }

    public RoomNotFoundException(Exception logError, string message)
        : base(message, NotFoundCode)
    {
        LogError = logError;
    }
}

public class ReservationNotFoundException : BaseApiException
{
    private const string DefaultMessage = "Reserve not found";
    private const int NotFoundCode = 404;

    public ReservationNotFoundException(Exception? logError)
        : base(DefaultMessage, NotFoundCode)
    {
        LogError = logError;
    }

    public ReservationNotFoundException(Exception? logError, string message)
        : base(message, NotFoundCode)
    {
        LogError = logError;
    }
}

public class RoleNotFoundException : BaseApiException
{
    private const string DefaultMessage = "Role not found";
    private const int NotFoundCode = 404;

    public RoleNotFoundException(Exception? logError)
        : base(DefaultMessage, NotFoundCode)
    {
        LogError = logError;
    }

    public RoleNotFoundException(Exception? logError, string message)
        : base(message, NotFoundCode)
    {
        LogError = logError;
    }
}

public class PaymentNotFoundException : BaseApiException
{
    private const string DefaultMessage = "Payment not found";
    private const int NotFoundCode = 404;

    public PaymentNotFoundException(Exception? logError)
        : base(DefaultMessage, NotFoundCode)
    {
        LogError = logError;
    }

    public PaymentNotFoundException(Exception? logError, string message)
        : base(message, NotFoundCode)
    {
        LogError = logError;
    }
}
