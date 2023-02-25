using System;

namespace Urdep.Extensions.Augmentation
{
    public static class Augment
    {
        public static AugmentedAny<T> Any<T>(T thing) => new AugmentedAny<T>(thing);

        public static AugmentedRef<T> C<T>(T thing)
            where T : class? => new AugmentedRef<T>(thing);

        public static AugmentedNotNull<T> D<T>(T thing)
            where T : notnull => new AugmentedNotNull<T>(thing);

        public static AugmentedStruct<T> S<T>(T thing)
            where T : struct => new AugmentedStruct<T>(thing);

        public static AugmentedNullableStruct<T> N<T>(T? thing)
            where T : struct => new AugmentedNullableStruct<T>(thing);
    }
}
