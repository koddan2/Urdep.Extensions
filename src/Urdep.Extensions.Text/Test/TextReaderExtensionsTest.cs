using NUnit.Framework;
using System.Text;

namespace Urdep.Extensions.Text.Test;

public class TextReaderExtensionsTest
{
    [Test]
    public void TestBasic1()
    {
        const string text =
            @"Hello
--[[SKIP
some gibberish
--]]
Important data
--[[SKIP
some other gibberish
--]]
--[[SKIP

some other gibberish again

--]]
";
        const string expected =
            @"Hello
Important data
";
        using var reader = new StringReader(text);
        var sb = new StringBuilder();
        var state = new TextReaderBlockSkipState("--[[SKIP", "--]]");

        state.ReadInto(sb, reader);
        var actual = sb.ToString();

        Assert.That(actual, Is.EqualTo(expected));
    }
}
