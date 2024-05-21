namespace Urdep.Extensions.Augmentation
{
    /// <summary>
    /// Augment helper methods.
    /// </summary>
    public static class Augment
    {
        /// <summary>
        /// Any augmented value.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="thing">The value.</param>
        /// <returns>The augmented value.</returns>
        public static AugmentedAny<T> Any<T>(T thing) => new(thing);

        /// <summary>
        /// Any augmented value.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="thing">The value.</param>
        /// <returns>The augmented value.</returns>
        public static AugmentedRef<T> Ref<T>(T thing)
            where T : class? => new(thing);

        /// <summary>
        /// Any augmented value.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="thing">The value.</param>
        /// <returns>The augmented value.</returns>
        public static AugmentedNotNull<T> NotNull<T>(T thing)
            where T : notnull => new(thing);

        /// <summary>
        /// Any augmented value.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="thing">The value.</param>
        /// <returns>The augmented value.</returns>
        public static AugmentedStruct<T> Struct<T>(T thing)
            where T : struct => new(thing);

        /// <summary>
        /// Any augmented value.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="thing">The value.</param>
        /// <returns>The augmented value.</returns>
        public static AugmentedNullableStruct<T> NullableStruct<T>(T? thing)
            where T : struct => new(thing);
    }
}
