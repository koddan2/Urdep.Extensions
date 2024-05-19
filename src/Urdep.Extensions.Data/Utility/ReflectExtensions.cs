using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Urdep.Extensions.Data.Utility;

/// <summary>
/// Extensions for reflection.
/// </summary>
public static class ReflectExtensions
{
    /// <summary>
    /// Get diagnostic data for the target of the invocation.
    /// Side-effect free.
    /// </summary>
    /// <param name="maybeValue">The value.</param>
    /// <param name="theSameValue">The same value.</param>
    /// <param name="message">Any extra message.</param>
    /// <param name="context">The context.</param>
    /// <param name="memberName">The name.</param>
    /// <param name="sourceFilePath">The path to the file.</param>
    /// <param name="sourceLineNumber">The line number in the file.</param>
    /// <typeparam name="T">The type of the expression.</typeparam>
    /// <returns>The string representing the code.</returns>
    [DebuggerStepThrough]
    public static string Here<T>(
        this T? maybeValue,
        out T? theSameValue,
        string? message = null,
        [CallerArgumentExpression(nameof(maybeValue))] string? context = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    )
    {
        theSameValue = maybeValue;
        var defaultValue = string.Empty;
        var msg = message is null ? defaultValue : $" '{message}'";
        var result =
            $"[{typeof(T).Name} {context ?? defaultValue}]{msg} (in {memberName}) → {sourceFilePath}:{sourceLineNumber.ToString(CultureInfo.InvariantCulture)}";
        return result;
    }

    /// <summary>
    /// Use this extension to unwrap a nullable thing and fail with exception if it is null.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="maybeValue">The reference.</param>
    /// <param name="message">the message.</param>
    /// <param name="exceptionMaker">innerException => newException.</param>
    /// <param name="context">leave empty (compiler assigns the value).</param>
    /// <param name="memberName">leave memberName empty (compiler assigns the value).</param>
    /// <param name="sourceFilePath">leave sourceFilePathempty (compiler assigns the value).</param>
    /// <param name="sourceLineNumber">leave sourceLineNumber empty (compiler assigns the value).</param>
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
            $"{(message is not null ? message + " - " : "")}expression: ({context}) (in member {memberName}) {sourceFilePath}:{sourceLineNumber.ToString(CultureInfo.InvariantCulture)}";
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
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="maybeValue">The reference.</param>
    /// <param name="message">the message.</param>
    /// <param name="exceptionMaker">innerException => newException.</param>
    /// <param name="context">leave empty (compiler assigns the value).</param>
    /// <param name="memberName">leave memberName empty (compiler assigns the value).</param>
    /// <param name="sourceFilePath">leave sourceFilePathempty (compiler assigns the value).</param>
    /// <param name="sourceLineNumber">leave sourceLineNumber empty (compiler assigns the value).</param>
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
            $"{(message is not null ? message + " - " : "")}expression: ({context}) (in member {memberName}) {sourceFilePath}:{sourceLineNumber.ToString(CultureInfo.InvariantCulture)}";
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
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="maybeValue">The reference.</param>
    /// <param name="message">the message.</param>
    /// <param name="exceptionMaker">innerException => newException.</param>
    /// <param name="context">leave empty (compiler assigns the value).</param>
    /// <param name="memberName">leave memberName empty (compiler assigns the value).</param>
    /// <param name="sourceFilePath">leave sourceFilePathempty (compiler assigns the value).</param>
    /// <param name="sourceLineNumber">leave sourceLineNumber empty (compiler assigns the value).</param>
    /// <returns>the non-null value.</returns>
    /// <exception cref="InvariantFailedException">If the value or reference is null.</exception>
    [DebuggerStepThrough]
    [return: NotNull]
    public static Task<T> OrFailAsync<T>(
        this Task<T> maybeValue,
        string? message = null,
        Func<Exception, Exception>? exceptionMaker = null,
        [CallerArgumentExpression(nameof(maybeValue))] string? context = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    )
    {
        string GetMsg() =>
            $"{(message is not null ? message + " - " : "")}expression: ({context}) (in member {memberName}) {sourceFilePath}:{sourceLineNumber.ToString(CultureInfo.InvariantCulture)}";

        return maybeValue.ContinueWith(
            tsk =>
            {
                var result = tsk.Result;
                return result
                    ?? (
                        exceptionMaker is null
                            ? throw new InvariantFailedException(GetMsg())
                            : throw exceptionMaker(new InvariantFailedException(GetMsg()))
                    );
            },
            scheduler: TaskScheduler.Default
        );
    }
}
