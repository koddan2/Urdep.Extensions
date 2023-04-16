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
    /// <seealso cref="https://github.com/dotnet/roslyn/issues/47066#issuecomment-1124926236"/>
    /// </summary>
    [Serializable]
    public class InvariantFailedException : Exception
    {
        public InvariantFailedException() { }

        public InvariantFailedException(string message)
            : base(message) { }

        public InvariantFailedException(string message, Exception inner)
            : base(message, inner) { }

        protected InvariantFailedException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context
        )
            : base(info, context) { }
    }
}
