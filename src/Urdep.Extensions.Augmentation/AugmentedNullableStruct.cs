using System;

namespace Urdep.Extensions.Augmentation
{
    public readonly record struct AugmentedNullableStruct<T>(T? Value) : IAugmented<T?>
        where T : struct;
}
