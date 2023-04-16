using NUnit.Framework;
using System;
using Urdep.Extensions.Data.Utility;

namespace Urdep.Extensions.Data.Test;

public class UtilityTest
{
    [Test]
    public void TestHere()
    {
        static T? GetValue<T>(IEnumerable<T> source)
        {
            return source.FirstOrDefault();
        }
        var arr = new[] { "" };
        var message = GetValue(arr).Here(out var result, "TEST");
        Assert.Multiple(() =>
        {
            Assert.That(message.StartsWith("<String>[GetValue(arr)] 'TEST'"));
            Assert.That(result, Is.EqualTo(arr[0]));
        });
    }

    [Test]
    public void TestOrFail1()
    {
        const string? nullValue = null;
        var exn = Assert.Throws<InvariantFailedException>(() => nullValue.OrFail());
        Assert.NotNull(exn);
    }

    [Test]
    public void TestOrFail2()
    {
        const string? notNullValue = "";
        string guaranteedNotNull = notNullValue.OrFail();
        string noWarning = guaranteedNotNull.ToUpper();
        Assert.NotNull(noWarning);
    }

    [Test]
    public void TestOrFail3()
    {
        int? nullValue = null;
        var exn = Assert.Throws<InvariantFailedException>(() => nullValue.OrFail());
        Assert.NotNull(exn);
    }

    [Test]
    public void TestOrFail4()
    {
        int? notNullValue = (int?)0;
        int guaranteedNotNull = notNullValue.OrFail();
        int noWarning = guaranteedNotNull.CompareTo(3);
        Assert.NotNull(noWarning);
    }
}
