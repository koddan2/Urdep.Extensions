using NUnit.Framework;
using Urdep.Extensions.Augmentation;
using Urdep.Extensions.Data;
using Urdep.Extensions.Data.Utility;

namespace Tests.Data;

/// <summary>
/// Tests for object extensions.
/// </summary>
public class ObjectExtensionsTest
{
    /// <summary>
    /// a test class.
    /// </summary>
    public class MyTestClass
    {
        /// <summary>
        /// an int value
        /// </summary>
        public int IntValue { get; set; }
        /// <summary>
        /// a string value
        /// </summary>
        public string? StrValue { get; set; }
        /// <summary>
        /// a bool value
        /// </summary>
        public bool BoolValue { get; set; }
        /// <summary>
        /// a reference value
        /// </summary>
        public object? RefValue { get; set; }
    }

    /// <summary>
    /// Tests the basic transform
    /// </summary>
    [Test]
    public void TestBasicTransform()
    {
        var startInstance = new MyTestClass
        {
            BoolValue = true,
            IntValue = 0,
            StrValue = "Hello",
            RefValue = new()
        };
        var aug = Augment.Ref(startInstance);
        var dict = aug.AsDictionary();
        Assert.Multiple(() =>
        {
            Assert.That(startInstance, Is.EqualTo(aug.Value));
            Assert.That(dict.Keys, Has.Count.EqualTo(4));

            Assert.That(dict["IntValue"], Is.EqualTo(aug.Value.IntValue));
            Assert.That(dict["RefValue"], Is.EqualTo(aug.Value.RefValue));
        });

        var newInstance = dict.TransformInto<MyTestClass>();
        Assert.Multiple(() =>
        {
            Assert.That(newInstance, Is.Not.EqualTo(aug.Value));
            Assert.That(newInstance.StrValue, Is.EqualTo(dict["StrValue"]));
            Assert.That(newInstance.RefValue, Is.EqualTo(dict["RefValue"]));
        });
    }

    /// <summary>
    /// Tests the .Tap extension method.
    /// </summary>
    [Test]
    public void TestTap1()
    {
        var a = Augment.Ref(new List<int> { 1, 2, 3 });
        Assert.That(a.Value, Has.Count.EqualTo(3));
        var b = a.Tap(list => list.Add(4));
        Assert.That(a.Value, Has.Count.EqualTo(4));
        Assert.Multiple(() =>
        {
            Assert.That(a.Value, Is.EqualTo(b.Value));
            Assert.That(a, Is.EqualTo(b));
        });
    }
}
