namespace Urdep.Extensions.Augmentation
{
    /// <summary>
    /// An augmented value.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="Value">The value.</param>
    public record AugmentedAny<T>(T Value) : IAugmented<T>;
}
