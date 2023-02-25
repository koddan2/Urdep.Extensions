namespace Urdep.Extensions.Augmentation
{
    public record AugmentedRef<T>(T Value) : IAugmented<T>
        where T : class?;
}
