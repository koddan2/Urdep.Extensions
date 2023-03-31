﻿using NUnit.Framework;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Urdep.Extensions.Data.Test;

public class DynamicJsonHelperTest
{
    public record Thing1(int Value);

    public record Thing2(string Value);

    public DynamicJsonHelper Helper { get; } =
        new DynamicJsonHelper("TypeName").MapMany(
            ("Thing1", typeof(Thing1)),
            ("Thing2", typeof(Thing2))
        );

    [Test]
    public void BaseCase1()
    {
        const string json = @"{""TypeName"": ""Thing1"", ""Value"": 1}";
        var obj = GenericTest<Thing1>(json);
        Assert.That(obj!.Value, Is.EqualTo(1));
    }

    [Test]
    public void BaseCase2()
    {
        const string json = @"{""TypeName"": ""Thing2"", ""Value"": ""TEST""}";
        var obj = GenericTest<Thing2>(json);
        Assert.That(obj!.Value, Is.EqualTo("TEST"));
    }

    private T? GenericTest<T>(string json)
    {
        var jsonNode = JsonSerializer.Deserialize<JsonNode>(json);
        Assert.That(jsonNode, Is.Not.Null);
        if (Helper.Is<T>(jsonNode!))
        {
            var result = jsonNode.Deserialize<T>();
            Assert.That(result, Is.Not.Null);
            return result;
        }

        throw new Exception("Wrong type");
    }
}
