// Copyright © Theodore Tsirpanis and Contributors.
// Licensed under the MIT License (MIT).
// See LICENSE in the repository root for more information.

namespace Recursiont.Tests;

[TestFixture]
internal class RecursiveRunnerTests
{
    [Test]
    public void DeepRecursion([Values(10u, 10_000u)] uint n)
    {
        uint actual = new RecursiveRunner().Run(Impl, n);
        Assert.That(actual, Is.EqualTo(n));

        static async RecursiveOp<uint> Impl(uint n)
        {
            if (n == 0)
            {
                return 0;
            }
            return await Impl(n - 1) + 1;
        }
    }

    [TestCase(20u, ExpectedResult = 6765u)]
    [TestCase(25u, ExpectedResult = 75025u)]
    [TestCase(26u, ExpectedResult = 121393u)]
    public uint FibonacciNumbers(uint n)
    {
        return new RecursiveRunner().Run(Impl, n);

        static async RecursiveOp<uint> Impl(uint n) => n switch
        {
            0 => 0,
            1 => 1,
            _ => await Impl(n - 1) + await Impl(n - 2)
        };
    }

    [Test]
    public void EnforcesAwaitingImmediately()
    {
        Assert.Throws<InvalidOperationException>(() => new RecursiveRunner().Run(Impl, true));

        static async RecursiveOp Impl(bool doRecurse)
        {
            await RecursiveOp.Yield();

            if (!doRecurse)
            {
                return;
            }
            _ = Impl(false);
            await Impl(false);
        }
    }

    [Test]
    public void RunOverloadsAvailable()
    {
        RecursiveRunner runner = new();

        // The Run overloads are source-generated; we just want to ensure they exist.
        runner.Run(() => RecursiveOp.CompletedOp);
        int result = runner.Run(() => RecursiveOp.FromResult(5));
        Assert.That(result, Is.EqualTo(5));

        runner.Run(_ => RecursiveOp.CompletedOp, 0);
        result = runner.Run(_ => RecursiveOp.FromResult(5), 0);
        Assert.That(result, Is.EqualTo(5));

        runner.Run((_, _) => RecursiveOp.CompletedOp, 0, 0);
        result = runner.Run((_, _) => RecursiveOp.FromResult(5), 0, 0);
        Assert.That(result, Is.EqualTo(5));

        runner.Run((_, _, _) => RecursiveOp.CompletedOp, 0, 0, 0);
        result = runner.Run((_, _, _) => RecursiveOp.FromResult(5), 0, 0, 0);
        Assert.That(result, Is.EqualTo(5));

        runner.Run((_, _, _, _) => RecursiveOp.CompletedOp, 0, 0, 0, 0);
        result = runner.Run((_, _, _, _) => RecursiveOp.FromResult(5), 0, 0, 0, 0);
        Assert.That(result, Is.EqualTo(5));

        runner.Run((_, _, _, _, _) => RecursiveOp.CompletedOp, 0, 0, 0, 0, 0);
        result = runner.Run((_, _, _, _, _) => RecursiveOp.FromResult(5), 0, 0, 0, 0, 0);
        Assert.That(result, Is.EqualTo(5));

        runner.Run((_, _, _, _, _, _) => RecursiveOp.CompletedOp, 0, 0, 0, 0, 0, 0);
        result = runner.Run((_, _, _, _, _, _) => RecursiveOp.FromResult(5), 0, 0, 0, 0, 0, 0);
        Assert.That(result, Is.EqualTo(5));
    }
}
