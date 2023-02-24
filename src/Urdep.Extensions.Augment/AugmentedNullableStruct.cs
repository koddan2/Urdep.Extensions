using System;

namespace Urdep.Extensions.Augment
{
    public readonly record struct AugmentedNullableStruct<T>(Nullable<T> Value)
        : IAugmented<Nullable<T>>
        where T : struct;
}
