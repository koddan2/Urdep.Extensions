namespace Urdep.Extensions.Augmentation
{
    /// <summary>
    /// An augmented reference type.
    /// </summary>
    /// <typeparam name="T">The reference type.</typeparam>
    /// <param name="Value">The instance value.</param>
    public record AugmentedRef<T>(T Value) : IAugmented<T>
        where T : class?;
}
