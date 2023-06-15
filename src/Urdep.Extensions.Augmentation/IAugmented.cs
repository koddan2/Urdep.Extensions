namespace Urdep.Extensions.Augmentation
{
    /// <summary>
    /// An augmented T
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    public interface IAugmented<T>
    {
        /// <summary>
        /// The instance value.
        /// </summary>
        T Value { get; }
    }
}
