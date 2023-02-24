namespace Urdep.Extensions.Augment
{
    public record AugmentedNotNull<T>(T Value) : IAugmented<T>
        where T : notnull;
}
