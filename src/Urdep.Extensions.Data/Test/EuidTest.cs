using NUnit.Framework;
using System.Buffers.Text;
using System.Text.Json;

namespace Urdep.Extensions.Data.Test;

public class EuidTest
{
    [Test]
    public void Test1()
    {
        var g = new Guid("dd095a5f-dec6-434a-a0f7-a253e85fde14");
        Euid e = g;
        var bytes = g.ToByteArray();
        var checkStr = Base62.EncodingExtensions.ToBase62(bytes);
        var checkG = new Guid(Base62.EncodingExtensions.FromBase62(e.Value));

        Guid g2 = e;
        Assert.That(g, Is.EqualTo(g2));

        Console.WriteLine(checkG);
        Console.WriteLine(e);
        Assert.That(checkStr, Is.EqualTo((string)e));
        Assert.That(checkG, Is.EqualTo(g));
    }

    record TestData(Euid Euid);

    [Test]
    public void TestJson()
    {
        var g = new Guid("dd095a5f-dec6-434a-a0f7-a253e85fde14");
        Euid e = g;

        var data = new TestData(e);
        var data2 = new TestData(e);
        Assert.That(data, Is.EqualTo(data2));

        var json = JsonSerializer.Serialize(data);
        Assert.That(json, Is.EqualTo("{\"Euid\":\"2tvPxpmCKQi7ExfoioGDiO\"}"));
        var back = JsonSerializer.Deserialize<TestData>(json)!;

        Assert.That(back, Is.EqualTo(data));
    }

    [Test]
    public void Test3()
    {
        ////var rng = new Random();
        ////var bytes = Enumerable.Range(1, 0x1f).Select(_ => (byte)rng.Next(0xff)).ToArray();
        ////var bytes = Guid.NewGuid().ToByteArray().Concat(Guid.NewGuid().ToByteArray()).ToArray();
        var bytes = Guid.NewGuid().ToByteArray();
        var e = Euid.FromBytes(bytes);
        var b = Convert.ToBase64String(bytes);
        var c = Convert.ToBase64String(e.GetByteArray());
        Console.WriteLine(e);
        Console.WriteLine(b);
        Console.WriteLine(c);
        Assert.That(b, Is.EqualTo(c));
    }
}
