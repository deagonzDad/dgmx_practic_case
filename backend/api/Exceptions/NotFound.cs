using System;

namespace api.Exceptions;

public class UserNotFoundException : BaseApiException
{
    private const int NotFoundCode = 404;
    private const string DefaultMessage = "User not found.";

    public UserNotFoundException()
        : base(DefaultMessage, NotFoundCode) { }

    public UserNotFoundException(string message)
        : base(message, NotFoundCode) { }
}

public class RoomNotFound : BaseApiException
{
    private const string DefaultMessage = "Room not found";
    private const int NotFoundCode = 404;

    public RoomNotFound()
        : base(DefaultMessage, NotFoundCode) { }

    public RoomNotFound(string message)
        : base(message, NotFoundCode) { }
}

public class ReservationNotFound : BaseApiException
{
    private const string DefaultMessage = "Reserve not found";
    private const int NotFoundCode = 404;

    public ReservationNotFound()
        : base(DefaultMessage, NotFoundCode) { }

    public ReservationNotFound(string message)
        : base(message, NotFoundCode) { }
}
