namespace Urdep.Extensions.Augment
{
    public record AugmentedRef<T>(T Value) : IAugmented<T>
        where T : class?;
}
