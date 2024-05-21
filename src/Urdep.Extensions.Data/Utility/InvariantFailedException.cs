namespace Urdep.Extensions.Data.Utility
{
    /// <summary>
    /// Exception that indicates that a program error occured in that an invariant was violated.
    /// <example><code>
    /// enum E { A = 1, B = 2 }
    /// // ...
    /// var v = E.A;
    /// var n = v switch
    /// {
    ///     E.A => 1,
    ///     E.B => 2,
    ///     _ => throw new InvariantFailedException($"Value v was not a legal value: ({v})."),
    /// }
    /// </code>
    /// </example>
    /// <remarks>
    /// https://github.com/dotnet/roslyn/issues/47066#issuecomment-1124926236
    /// </remarks>
    /// </summary>
    [Serializable]
    public class InvariantFailedException : Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public InvariantFailedException() { }

        /// <summary>
        /// Constructor with a message.
        /// </summary>
        /// <param name="message">The message</param>
        public InvariantFailedException(string message)
            : base(message) { }

        /// <summary>
        /// Constructor with a message and an inner exception.
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="inner">The inner exception</param>
        public InvariantFailedException(string message, Exception inner)
            : base(message, inner) { }
    }
}
