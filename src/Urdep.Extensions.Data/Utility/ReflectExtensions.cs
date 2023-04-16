using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Urdep.Extensions.Data.Utility;

/// <summary>
/// Extensions for reflection.
/// </summary>
//[SuppressMessage("Roslynator", "RCS1175:Unused 'this' parameter.", Justification = "Used by compiler")]
//[SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used by compiler")]
public static class ReflectExtensions
{
    /// <summary>
    /// Get diagnostic data for the target of the invocation.
    /// </summary>
    /// <example>
    /// </example>
    [DebuggerStepThrough]
    [return: NotNull]
    public static string Here<T>(
        this T? maybeValue,
        out T? output,
        string? message = null,
        [CallerArgumentExpression(nameof(maybeValue))] string? context = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    )
    {
        var defaultValue = string.Empty;
        var msg = message is null ? defaultValue : $" '{message}'";
        var result =
            $"<{typeof(T).Name}>[{context ?? defaultValue}]{msg} (in {memberName}) → {sourceFilePath}:{sourceLineNumber}";
        output = maybeValue;
        return result ?? "";
    }

    /// <summary>
    /// Use this extension to unwrap a nullable thing and fail with exception if it is null.
    /// </summary>
    /// <typeparam name="T">The type</typeparam>
    /// <param name="maybeValue">The reference</param>
    /// <param name="message">the message</param>
    /// <param name="exceptionMaker">innerException => newException</param>
    /// <param name="context">leave empty (compiler assigns the value)</param>
    /// <param name="memberName">leave memberName empty (compiler assigns the value)</param>
    /// <param name="sourceFilePath">leave sourceFilePathempty (compiler assigns the value)</param>
    /// <param name="sourceLineNumber">leave sourceLineNumber empty (compiler assigns the value)</param>
    /// <returns>the non-null value.</returns>
    /// <exception cref="InvariantFailedException">If the value or reference is null.</exception>
    [DebuggerStepThrough]
    [return: NotNull]
    public static T OrFail<T>(
        this T? maybeValue,
        string? message = null,
        Func<Exception, Exception>? exceptionMaker = null,
        [CallerArgumentExpression(nameof(maybeValue))] string? context = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    )
        where T : struct
    {
        string GetMsg() =>
            $"{(message is not null ? message + " - " : "")}expression: ({context}) (in member {memberName}) {sourceFilePath}:{sourceLineNumber}";
        return maybeValue
            ?? (
                exceptionMaker is null
                    ? throw new InvariantFailedException(GetMsg())
                    : throw exceptionMaker(new InvariantFailedException(GetMsg()))
            );
    }

    /// <summary>
    /// Use this extension to unwrap a nullable thing and fail with exception if it is null.
    /// </summary>
    /// <typeparam name="T">The type</typeparam>
    /// <param name="maybeValue">The reference</param>
    /// <param name="message">the message</param>
    /// <param name="exceptionMaker">innerException => newException</param>
    /// <param name="context">leave empty (compiler assigns the value)</param>
    /// <param name="memberName">leave memberName empty (compiler assigns the value)</param>
    /// <param name="sourceFilePath">leave sourceFilePathempty (compiler assigns the value)</param>
    /// <param name="sourceLineNumber">leave sourceLineNumber empty (compiler assigns the value)</param>
    /// <returns>the non-null value.</returns>
    /// <exception cref="InvariantFailedException">If the value or reference is null.</exception>
    [DebuggerStepThrough]
    [return: NotNull]
    public static T OrFail<T>(
        this T? maybeValue,
        string? message = null,
        Func<Exception, Exception>? exceptionMaker = null,
        [CallerArgumentExpression(nameof(maybeValue))] string? context = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    )
        where T : class
    {
        string GetMsg() =>
            $"{(message is not null ? message + " - " : "")}expression: ({context}) (in member {memberName}) {sourceFilePath}:{sourceLineNumber}";
        return maybeValue
            ?? (
                exceptionMaker is null
                    ? throw new InvariantFailedException(GetMsg())
                    : throw exceptionMaker(new InvariantFailedException(GetMsg()))
            );
    }

    /// <summary>
    /// Use this extension to unwrap an async nullable thing and fail with exception if it is null.
    /// </summary>
    /// <typeparam name="T">The type</typeparam>
    /// <param name="maybeValue">The reference</param>
    /// <param name="message">the message</param>
    /// <param name="exceptionMaker">innerException => newException</param>
    /// <param name="context">leave empty (compiler assigns the value)</param>
    /// <param name="memberName">leave memberName empty (compiler assigns the value)</param>
    /// <param name="sourceFilePath">leave sourceFilePathempty (compiler assigns the value)</param>
    /// <param name="sourceLineNumber">leave sourceLineNumber empty (compiler assigns the value)</param>
    /// <returns>the non-null value.</returns>
    /// <exception cref="InvariantFailedException">If the value or reference is null.</exception>
    [DebuggerStepThrough]
    [return: NotNull]
    public static async Task<T> OrFailAsync<T>(
        this Task<T?> maybeValue,
        string? message = null,
        Func<Exception, Exception>? exceptionMaker = null,
        [CallerArgumentExpression(nameof(maybeValue))] string? context = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    )
        where T : class
    {
        string GetMsg() =>
            $"{(message is not null ? message + " - " : "")}expression: ({context}) (in member {memberName}) {sourceFilePath}:{sourceLineNumber}";
        return await maybeValue
            ?? (
                exceptionMaker is null
                    ? throw new InvariantFailedException(GetMsg())
                    : throw exceptionMaker(new InvariantFailedException(GetMsg()))
            );
    }
}
