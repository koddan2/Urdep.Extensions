namespace Urdep.Extensions.Augmentation
{
    /// <summary>
    /// An augmented value, that is not null.
    /// </summary>
    /// <typeparam name="T">The not null type.</typeparam>
    /// <param name="Value">The not null value.</param>
    public record AugmentedNotNull<T>(T Value) : IAugmented<T>
        where T : notnull;
}
