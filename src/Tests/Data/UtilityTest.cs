using NUnit.Framework;
using System;
using Urdep.Extensions.Data.Utility;

namespace Tests.Data;

/// <summary>
/// Tests for Utility classes in Data.
/// </summary>
public class UtilityTest
{
    /// <summary>
    /// Test that the extension method .Here works.
    /// </summary>
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
            Assert.That(message, Does.StartWith("<String>[GetValue(arr)] 'TEST'"));
            Assert.That(result, Is.EqualTo(arr[0]));
        });
    }

    /// <summary>
    /// Test that .OrFail throws when it should.
    /// </summary>
    [Test]
    public void TestOrFail1()
    {
        const string? nullValue = null;
        var exn = Assert.Throws<InvariantFailedException>(() => nullValue.OrFail());
        Assert.That(exn, Is.Not.Null);
    }

    /// <summary>
    /// Test that there are no warnings when using .OrFail
    /// </summary>
    [Test]
    public void TestOrFail2()
    {
        const string? notNullValue = "";
        string guaranteedNotNull = notNullValue.OrFail();
        string noWarning = guaranteedNotNull.ToUpper();
        Assert.That(noWarning, Is.Not.Null);
    }

    /// <summary>
    /// Test that .OrFail throws.
    /// </summary>
    [Test]
    public void TestOrFail3()
    {
        int? nullValue = null;
        var exn = Assert.Throws<InvariantFailedException>(() => nullValue.OrFail());
        Assert.That(exn, Is.Not.Null);
    }

    /// <summary>
    /// Test that .OrFail guarantees not null value.
    /// </summary>
    [Test]
    public void TestOrFail4()
    {
        int? notNullValue = 0;
        int guaranteedNotNull = notNullValue.OrFail();
        int noWarning = guaranteedNotNull.CompareTo(3);
        Assert.That((object?)noWarning, Is.Not.Null);
    }
}
