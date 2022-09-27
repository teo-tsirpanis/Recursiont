// Copyright © Theodore Tsirpanis and Contributors.
// Licensed under the MIT License (MIT).
// See LICENSE in the repository root for more information.

using BenchmarkDotNet.Attributes;

namespace Recursiont.Benchmarks;

public class ShallowRecursionBenchmark
{
    public const uint N = 20;

    [Benchmark]
    public int Plain()
    {
        return Impl(N);

        static int Impl(uint n)
        {
            if (n == 0)
            {
                return 1;
            }

            return Impl(n - 1) + Impl(n - 1);
        }
    }

    [Benchmark(Baseline = true)]
    public int Recursiont()
    {
        return RecursiveRunner.Run(Impl, N);

        static async RecursiveOp<int> Impl(uint n)
        {
            if (n == 0)
            {
                return 1;
            }

            return await Impl(n - 1) + await Impl(n - 1);
        }
    }
}
