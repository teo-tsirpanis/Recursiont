// Copyright © Theodore Tsirpanis and Contributors.
// Licensed under the MIT License (MIT).
// See LICENSE in the repository root for more information.

using BenchmarkDotNet.Attributes;
using System.Runtime.CompilerServices;

namespace Recursiont.Benchmarks;

[MemoryDiagnoser, RankColumn]
public class TreeTraversalBenchmark
{
    [Params(10u, 1_000u, 100_000u)]
    public uint Size { get; set; }

    private Tree? _tree;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _tree = GenerateTree(Size);
    }

    [Benchmark(Baseline = true)]
    public int Recursiont()
    {
        return RecursiveRunner.Run(Impl, _tree);

        static async RecursiveOp<int> Impl(Tree? tree)
        {
            if (tree is null)
            {
                return 0;
            }

            return await Impl(tree.Left) + await Impl(tree.Right) + 1;
        }
    }

    [Benchmark]
    public int Tasks()
    {
        return Impl(_tree);

        static int Impl(Tree? tree)
        {
            if (tree is null)
            {
                return 0;
            }

            if (!RuntimeHelpers.TryEnsureSufficientExecutionStack())
            {
                return Task.Run(() => Impl(tree)).GetAwaiter().GetResult();
            }

            return Impl(tree.Left) + Impl(tree.Right) + 1;
        }
    }

    private static Tree GenerateTree(uint size)
    {
        var root = new Tree();
        var current = root;
        bool left = true;
        while (size-- > 0)
        {
            Tree deep = new Tree();
            Tree shallow = new Tree() {Left = new Tree() {Right = new Tree()}};
            if (left)
            {
                current.Left = deep;
                current.Right = shallow;
            }
            else
            {
                current.Left = shallow;
                current.Right = deep;
            }
            current = deep;
            left = !left;
        }
        return root;
    }

    private sealed class Tree
    {
        public Tree? Left { get; set; }
        public Tree? Right { get; set; }
    }
}
