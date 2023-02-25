namespace Urdep.Extensions.Augmentation
{
    public record AugmentedNotNull<T>(T Value) : IAugmented<T>
        where T : notnull;
}
