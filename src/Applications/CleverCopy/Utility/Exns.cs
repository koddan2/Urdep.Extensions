namespace TrackingCopyTool.Utility;

internal static class Exns
{
    private const string _DefaultMessage = "Unknown error";

    public static Exception GeneralError(int errorCode, string? message)
        => new ApplicationException($"Error ({errorCode}): {message ?? _DefaultMessage}")
        {
            Data =
            {
                ["ErrorCode"] = errorCode,
            }
        };
}
