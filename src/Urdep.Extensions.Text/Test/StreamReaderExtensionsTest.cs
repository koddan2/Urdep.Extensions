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
        var stream = new MemoryStream(Encoding.Default.GetBytes(text));
        var reader = new StreamReader(stream);
        var sb = new StringBuilder();
        var state = new StreamReaderSkipBlocksState("--[[SKIP", "--]]");
        while (!reader.EndOfStream)
        {
            var line = (reader, state).ReadLine();
            if (line is not null)
            {
                sb.AppendLine(line);
            }
        }
        var actual = sb.ToString();

        Assert.Multiple(() =>
        {
            Assert.That(actual, Is.EqualTo(expected));
        });
    }
}
