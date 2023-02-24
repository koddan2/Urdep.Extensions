namespace Urdep.Extensions.Augment
{
    public readonly record struct AugmentedStruct<T>(T Value) : IAugmented<T>
        where T : struct;
}
