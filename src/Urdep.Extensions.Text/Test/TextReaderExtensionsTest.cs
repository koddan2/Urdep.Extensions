using NUnit.Framework;
using System.Text;

namespace Urdep.Extensions.Text.Test;

public class StreamReaderExtensionsTest
{
    [Test]
    public void TestBasic1()
    {
        var text =
            @"Hello
--[[SKIP
some gibberish
--]]
Important data
";
        var expected =
            @"Hello
Important data
";
        using var reader = new StringReader(text);
        var sb = new StringBuilder();
        var state = new TextReaderBlockSkipState("--[[SKIP", "--]]");
        TextReaderBlockSkipResult readResult;
        do
        {
            readResult = state.ReadLine(reader);
            if (!readResult.Skip)
            {
                sb.AppendLine(readResult.Line);
            }
        } while (readResult.Line is not null);
        var actual = sb.ToString();

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void TestBasic2()
    {
        var text =
            @"Hello
--[[SKIP
some gibberish
--]]
Important data

--[[SKIP
--]]
--[[SKIP
some MORE gibberish
--]]
TEST~
";
        var expected =
            @"Hello
Important data

TEST~
";
        using var reader = new StringReader(text);
        var state = new TextReaderBlockSkipState("--[[SKIP", "--]]");
        var actual = state.ReadAll(reader);

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void TestBasic3()
    {
        var text =
            @"Hello
--[[SKIP
some gibberish
--]]
Important data
";
        var expected =
            @"Hello
Important data
";
        using var reader = new StringReader(text);
        var sb = new StringBuilder();
        var state = new TextReaderBlockSkipState("--[[SKIP", "--]]");

        for (
            TextReaderBlockSkipResult readResult = new(true, string.Empty);
            readResult.Line is not null;
            readResult = state.ReadLine(reader)
        )
        {
            if (!readResult.Skip)
            {
                sb.AppendLine(readResult.Line);
            }
        }
        var actual = sb.ToString();

        Assert.That(actual, Is.EqualTo(expected));
    }
}
