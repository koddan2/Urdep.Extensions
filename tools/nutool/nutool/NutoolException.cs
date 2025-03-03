namespace nutool;

public class NutoolException : System.Exception
{
    public NutoolException() { }

    public NutoolException(string message)
        : base(message) { }

    public NutoolException(string message, System.Exception inner)
        : base(message, inner) { }
}
