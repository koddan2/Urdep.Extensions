namespace Urdep.Extensions.Augmentation
{
    /// <summary>
    /// An augmented nullable struct.
    /// </summary>
    /// <typeparam name="T">The nullable struct type.</typeparam>
    /// <param name="Value">The value.</param>
    public readonly record struct AugmentedNullableStruct<T>(T? Value) : IAugmented<T?>
        where T : struct;
}
