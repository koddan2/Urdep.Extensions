namespace Urdep.Extensions.Augment
{
    public record AugmentedAny<T>(T Value) : IAugmented<T>;
}
