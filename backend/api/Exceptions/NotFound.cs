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

public class RoomNotFoundException : BaseApiException
{
    private const string DefaultMessage = "Room not found";
    private const int NotFoundCode = 404;

    public RoomNotFoundException()
        : base(DefaultMessage, NotFoundCode) { }

    public RoomNotFoundException(string message)
        : base(message, NotFoundCode) { }
}

public class ReservationNotFoundException : BaseApiException
{
    private const string DefaultMessage = "Reserve not found";
    private const int NotFoundCode = 404;

    public ReservationNotFoundException()
        : base(DefaultMessage, NotFoundCode) { }

    public ReservationNotFoundException(string message)
        : base(message, NotFoundCode) { }
}

public class RoleNotFoundException : BaseApiException
{
    private const string DefaultMessage = "Role not found";
    private const int NotFoundCode = 404;

    public RoleNotFoundException()
        : base(DefaultMessage, NotFoundCode) { }

    public RoleNotFoundException(string message)
        : base(message, NotFoundCode) { }
}
