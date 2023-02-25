namespace Urdep.Extensions.Augmentation
{
    public record AugmentedAny<T>(T Value) : IAugmented<T>;
}
