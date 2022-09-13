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
        uint actual = RecursiveRunner.Run(Impl, n);
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

    [TestCase(3u, 3u, ExpectedResult = 61u)]
    [TestCase(3u, 4u, ExpectedResult = 125u)]
    // The following takes extraordinarily long.
    // [TestCase(4u, 1u, ExpectedResult = 65533u)]
    public uint AckermannFunction(uint m, uint n)
    {
        return RecursiveRunner.Run(Impl, m, n);

        static async RecursiveOp<uint> Impl(uint m, uint n)
        {
            if (m == 0)
            {
                return n + 1;
            }
            if (n == 0)
            {
                return await Impl(m - 1, 1);
            }
            return await Impl(m - 1, await Impl(m, n - 1));
        }
    }

    [TestCase(20u, ExpectedResult = 6765u)]
    [TestCase(25u, ExpectedResult = 75025u)]
    [TestCase(26u, ExpectedResult = 121393u)]
    public uint FibonacciNumbers(uint n)
    {
        return RecursiveRunner.Run(Impl, n);

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
        Assert.Throws<InvalidOperationException>(() => RecursiveRunner.Run(Impl, true));

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
        // The Run overloads are source-generated; we just want to ensure they exist.
        RecursiveRunner.Run(() => RecursiveOp.CompletedOp);
        int result = RecursiveRunner.Run(() => RecursiveOp.FromResult(5));
        Assert.That(result, Is.EqualTo(5));

        RecursiveRunner.Run(_ => RecursiveOp.CompletedOp, 0);
        result = RecursiveRunner.Run(_ => RecursiveOp.FromResult(5), 0);
        Assert.That(result, Is.EqualTo(5));

        RecursiveRunner.Run((_, _) => RecursiveOp.CompletedOp, 0, 0);
        result = RecursiveRunner.Run((_, _) => RecursiveOp.FromResult(5), 0, 0);
        Assert.That(result, Is.EqualTo(5));

        RecursiveRunner.Run((_, _, _) => RecursiveOp.CompletedOp, 0, 0, 0);
        result = RecursiveRunner.Run((_, _, _) => RecursiveOp.FromResult(5), 0, 0, 0);
        Assert.That(result, Is.EqualTo(5));

        RecursiveRunner.Run((_, _, _, _) => RecursiveOp.CompletedOp, 0, 0, 0, 0);
        result = RecursiveRunner.Run((_, _, _, _) => RecursiveOp.FromResult(5), 0, 0, 0, 0);
        Assert.That(result, Is.EqualTo(5));

        RecursiveRunner.Run((_, _, _, _, _) => RecursiveOp.CompletedOp, 0, 0, 0, 0, 0);
        result = RecursiveRunner.Run((_, _, _, _, _) => RecursiveOp.FromResult(5), 0, 0, 0, 0, 0);
        Assert.That(result, Is.EqualTo(5));

        RecursiveRunner.Run((_, _, _, _, _, _) => RecursiveOp.CompletedOp, 0, 0, 0, 0, 0, 0);
        result = RecursiveRunner.Run((_, _, _, _, _, _) => RecursiveOp.FromResult(5), 0, 0, 0, 0, 0, 0);
        Assert.That(result, Is.EqualTo(5));
    }
}
