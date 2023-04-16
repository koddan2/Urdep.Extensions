﻿using NUnit.Framework;
using Urdep.Extensions.Augmentation;
using Urdep.Extensions.Data.Utility;

namespace Urdep.Extensions.Data.Test;

public class ObjectExtensionsTest
{
    public class MyTestClass
    {
        public int IntValue { get; set; }
        public string? StrValue { get; set; }
        public bool BoolValue { get; set; }
        public object? RefValue { get; set; }
    }

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

    [Test]
    public void TestTap1()
    {
        var a = Augment.Ref(new List<int> { 1, 2, 3 });
        Assert.That(a.Value.Count, Is.EqualTo(3));
        var b = a.Tap(list => list.Add(4));
        Assert.That(a.Value.Count, Is.EqualTo(4));
        Assert.That(a.Value, Is.EqualTo(b.Value));
        Assert.That(a, Is.EqualTo(b));
    }
}
