using Urdep.Extensions.Experiments;

namespace Tests.Experiments;

/// <summary>
/// Tests for experiments.
/// </summary>
public class ExperimentsTests
{
    /// <summary>
    /// Test 1
    /// </summary>
    [Test]
    public async Task Test1Async()
    {
        await Examples.Example1();
    }
}
