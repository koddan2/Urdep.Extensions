using System;

namespace Urdep.Extensions.Augmentation
{
    public static class Augment
    {
        public static AugmentedAny<T> Any<T>(T thing) => new(thing);

        public static AugmentedRef<T> Ref<T>(T thing)
            where T : class? => new(thing);

        public static AugmentedNotNull<T> NotNull<T>(T thing)
            where T : notnull => new(thing);

        public static AugmentedStruct<T> Struct<T>(T thing)
            where T : struct => new(thing);

        public static AugmentedNullableStruct<T> NullableStruct<T>(T? thing)
            where T : struct => new(thing);
    }
}
