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

        Assert.Multiple(() =>
        {
            Assert.That(actual, Is.EqualTo(expected));
        });
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
some MORE gibberish
--]]
";
        var expected =
            @"Hello
Important data

";
        using var reader = new StringReader(text);
        var state = new TextReaderBlockSkipState("--[[SKIP", "--]]");
        var actual = state.ReadAll(reader);

        Assert.Multiple(() =>
        {
            Assert.That(actual, Is.EqualTo(expected));
        });
    }
}
