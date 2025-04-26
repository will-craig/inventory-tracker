namespace Stockpile.Exceptions;

public class UserNotFoundException(string message) : Exception(message);
public class UsernameAlreadyTakenException : Exception;
public class EmailAlreadyTakenException : Exception;
