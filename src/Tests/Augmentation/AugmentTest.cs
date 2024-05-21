using System.Linq.Expressions;
using Urdep.Extensions.Augmentation;

namespace Tests.Augmentation;

/// <summary>
/// Tests for augment-related stuff.
/// </summary>
public class AugmentTest
{
    /// <summary>
    /// Test augmented struct.
    /// </summary>
    [Test]
    public void TestAugmentedStruct()
    {
        const int i = 0;

        Assert.Multiple(() =>
        {
            var a1 = Augment.Struct(i);
            var a2 = Augment.Struct(i);
            Assert.That(a1, Is.EqualTo(a2));
            object a = a1;
            Assert.That(a is AugmentedStruct<int>, Is.True);
            Assert.That(a is IAugmented<int>, Is.True);
        });
    }

    /// <summary>
    /// Augmented nullable test.
    /// </summary>
    [Test]
    public void TestAugmentedNullableStruct()
    {
        int? i = null;

        Assert.Multiple(() =>
        {
            var a1 = Augment.NullableStruct(i);
            var a2 = Augment.NullableStruct(i);
            Assert.That(a1, Is.EqualTo(a2));
            object a = a1;
            Assert.That(a is AugmentedNullableStruct<int>, Is.True);
            Assert.That(a is IAugmented<int?>, Is.True);
        });
    }

    /// <summary>
    /// Augmented nullable ref test.
    /// </summary>
    [Test]
    public void TestAugmentedNullableRef()
    {
        var i = new object();

        Assert.Multiple(() =>
        {
            AugmentedRef<object> a1 = Augment.Ref(i);
            Assert.That(a1, Is.Not.Null);
            var a2 = Augment.Ref(i);
            Assert.That(a1, Is.EqualTo(a2));
            Assert.That(a1 is IAugmented<object>);
        });
    }

    /// <summary>
    /// Augmented not null reference test.
    /// </summary>
    [Test]
    public void TestAugmentedNotNullRef()
    {
        var i = new object();

        Assert.Multiple(() =>
        {
            var a1 = Augment.NotNull(i);
            Assert.That(a1.GetType(), Is.EqualTo(typeof(AugmentedNotNull<object>)));
            var a2 = Augment.NotNull(i);
            Assert.That(a1, Is.EqualTo(a2));
            Assert.That(a1 is IAugmented<object>, Is.True);
        });
    }

    /// <summary>
    /// A creator of a class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class Creator<T>
    {
        /// <summary>
        /// The expression that constructs the object.
        /// </summary>
        public static readonly Func<T> Construct = Expression
            .Lambda<Func<T>>(Expression.New(typeof(T)))
            .Compile();
    }

    /// <summary>
    /// Non-generic creator
    /// </summary>
    public class Creator
    {
        /// <summary>
        /// The instance maker
        /// </summary>
        public readonly Func<object> Construct;

        /// <summary>
        /// Constructor for the Creator instance
        /// </summary>
        /// <param name="type">The type that should be created.</param>
        public Creator(Type type)
        {
            Type = type;
            Construct = Expression.Lambda<Func<object>>(Expression.New(Type)).Compile();
        }

        /// <summary>
        /// The type that this Creator instance makes.
        /// </summary>
        public Type Type { get; }
    }

    record TestRec1
    {
        public TestRec1()
        {
            MyProperty2 = 99;
        }

        public int MyProperty { get; set; } = 4;

        public int MyProperty2 { get; set; }
    }

    /// <summary>
    /// Test stack overflow example 1
    /// https://stackoverflow.com/a/29972767
    /// </summary>
    [Test]
    public void TestSO1()
    {
        static void DoTests(TestRec1 instance)
        {
            Assert.That(instance, Is.Not.Null);
            Assert.That(instance, Is.EqualTo(new TestRec1()));
        }
        var o1 = Creator<TestRec1>.Construct();
        DoTests(o1);
        var o2 = new Creator(typeof(TestRec1)).Construct();
        DoTests((TestRec1)o2);

        Assert.That(o1, Is.EqualTo(o2));
    }
}
