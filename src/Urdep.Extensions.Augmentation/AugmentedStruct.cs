namespace Urdep.Extensions.Augmentation
{
    /// <summary>
    /// An augmented struct.
    /// </summary>
    /// <typeparam name="T">The type</typeparam>
    /// <param name="Value">The value.</param>
    public readonly record struct AugmentedStruct<T>(T Value) : IAugmented<T>
        where T : struct;
}
