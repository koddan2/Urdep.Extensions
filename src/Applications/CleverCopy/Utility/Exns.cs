namespace TrackingCopyTool.Utility;

internal static class Exns
{
    private const string _DefaultMessage = "Unknown error";

    public static Exception GeneralError(int errorCode, string? message, params object?[]? args)
        => new ApplicationException($"Error ({errorCode}): {(message is not null ? string.Format(message, args ?? []) : _DefaultMessage)}")
        {
            Data =
            {
                ["ErrorCode"] = errorCode,
            }
        };
}
