namespace Urdep.Extensions.Augmentation
{
    public readonly record struct AugmentedStruct<T>(T Value) : IAugmented<T>
        where T : struct;
}
