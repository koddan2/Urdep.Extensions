using NUnit.Framework;
using System.Linq.Expressions;
using System;

namespace Urdep.Extensions.Augmentation.Test;

public class AugmentTest
{
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

    [Test]
    public void TestAugmentedNullableRef()
    {
        var i = new object();

        Assert.Multiple(() =>
        {
            var a1 = Augment.Ref(i);
#pragma warning disable IDE0150 // Prefer 'null' check over type check
            Assert.That(a1 is AugmentedRef<object>, Is.True);
#pragma warning restore IDE0150 // Prefer 'null' check over type check
            var a2 = Augment.Ref(i);
            Assert.That(a1, Is.EqualTo(a2));
            Assert.That(a1 is IAugmented<object>, Is.True);
        });
    }

    [Test]
    public void TestAugmentedNotNullRef()
    {
        var i = new object();

        Assert.Multiple(() =>
        {
            var a1 = Augment.NotNull(i);
#pragma warning disable IDE0150 // Prefer 'null' check over type check
            Assert.That(a1 is AugmentedNotNull<object>, Is.True);
#pragma warning restore IDE0150 // Prefer 'null' check over type check
            var a2 = Augment.NotNull(i);
            Assert.That(a1, Is.EqualTo(a2));
            Assert.That(a1 is IAugmented<object>, Is.True);
        });
    }

    public static class Creator<T>
    {
        public static readonly Func<T> Construct = Expression
            .Lambda<Func<T>>(Expression.New(typeof(T)))
            .Compile();
    }

    public class Creator
    {
        private readonly Type _type;
        public readonly Func<object> Construct;

        public Creator(Type type)
        {
            _type = type;
            Construct = Expression.Lambda<Func<object>>(Expression.New(_type)).Compile();
        }

        public Type Type => _type;
    }

    record TestRec1
    {
        public int MyProperty { get; set; } = 0;
    }

    [Test]
    public void TestSO1()
    {
        // https://stackoverflow.com/a/29972767
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
