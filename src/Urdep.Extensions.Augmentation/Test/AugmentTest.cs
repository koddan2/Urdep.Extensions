using NUnit;
using NUnit.Framework;

namespace Urdep.Extensions.Augmentation.Test;

public class AugmentTest
{
    [Test]
    public void TestAugmentedStruct()
    {
        var i = 0;

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
            Assert.That(a1 is AugmentedRef<object>, Is.True);
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
            Assert.That(a1 is AugmentedNotNull<object>, Is.True);
            var a2 = Augment.NotNull(i);
            Assert.That(a1, Is.EqualTo(a2));
            Assert.That(a1 is IAugmented<object>, Is.True);
        });
    }
}
