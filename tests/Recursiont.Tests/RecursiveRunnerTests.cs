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

    [TestCase(0, ExpectedResult = 1)]
    [TestCase(5, ExpectedResult = 0)]
    [TestCase(10, ExpectedResult = -67)]
    [TestCase(16, ExpectedResult = -7244)]
    public int ManOrBoy(int k)
    {
        return RecursiveRunner.Run(A, k, C(1), C(-1), C(-1), C(1), C(0));

        static Func<RecursiveOp<int>> C(int i) => () => RecursiveOp.FromResult(i);

        static async RecursiveOp<int> A(int k, Func<RecursiveOp<int>> x1, Func<RecursiveOp<int>> x2, Func<RecursiveOp<int>> x3, Func<RecursiveOp<int>> x4, Func<RecursiveOp<int>> x5)
        {
            RecursiveOp<int> b() { k--; return A(k, b, x1, x2, x3, x4); }

            return k <= 0 ? await x4() + await x5() : await b();
        }
    }

    [Test]
    public void EnforcesAwaitingImmediately()
    {
        Assert.That(() => RecursiveRunner.Run(Impl, true), Throws.InvalidOperationException);

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

        // Test allows ref struct on supported frameworks.
        var span =
#if NET9_0_OR_GREATER
            ReadOnlySpan<int>.Empty;
#else
            0;
#endif

        RecursiveRunner.Run(() => RecursiveOp.CompletedOp);
        int result = RecursiveRunner.Run(() => RecursiveOp.FromResult(5));
        Assert.That(result, Is.EqualTo(5));

        RecursiveRunner.Run(_ => RecursiveOp.CompletedOp, span);
        result = RecursiveRunner.Run(_ => RecursiveOp.FromResult(5), span);
        Assert.That(result, Is.EqualTo(5));

        RecursiveRunner.Run((_, _) => RecursiveOp.CompletedOp, 0, span);
        result = RecursiveRunner.Run((_, _) => RecursiveOp.FromResult(5), 0, span);
        Assert.That(result, Is.EqualTo(5));

        RecursiveRunner.Run((_, _, _) => RecursiveOp.CompletedOp, 0, 0, span);
        result = RecursiveRunner.Run((_, _, _) => RecursiveOp.FromResult(5), 0, 0, span);
        Assert.That(result, Is.EqualTo(5));

        RecursiveRunner.Run((_, _, _, _) => RecursiveOp.CompletedOp, 0, 0, 0, span);
        result = RecursiveRunner.Run((_, _, _, _) => RecursiveOp.FromResult(5), 0, 0, 0, span);
        Assert.That(result, Is.EqualTo(5));

        RecursiveRunner.Run((_, _, _, _, _) => RecursiveOp.CompletedOp, 0, 0, 0, 0, span);
        result = RecursiveRunner.Run((_, _, _, _, _) => RecursiveOp.FromResult(5), 0, 0, 0, 0, span);
        Assert.That(result, Is.EqualTo(5));

        RecursiveRunner.Run((_, _, _, _, _, _) => RecursiveOp.CompletedOp, 0, 0, 0, 0, 0, span);
        result = RecursiveRunner.Run((_, _, _, _, _, _) => RecursiveOp.FromResult(5), 0, 0, 0, 0, 0, span);
        Assert.That(result, Is.EqualTo(5));
    }
}
