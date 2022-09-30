// Copyright © Theodore Tsirpanis and Contributors.
// Licensed under the MIT License (MIT).
// See LICENSE in the repository root for more information.

using BenchmarkDotNet.Attributes;
using System.Runtime.ExceptionServices;

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

    [Benchmark]
    public int PlainWithTryCatch()
    {
        return Impl(N);

        static int Impl(uint n)
        {
            try
            {
                if (n == 0)
                {
                    return 1;
                }

                return Impl(n - 1) + Impl(n - 1);
            }
            catch (Exception e)
            {
                ExceptionDispatchInfo.Throw(e);
                return 0;
            }
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
