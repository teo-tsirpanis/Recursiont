// Copyright © Theodore Tsirpanis and Contributors.
// Licensed under the MIT License (MIT).
// See LICENSE in the repository root for more information.

using BenchmarkDotNet.Running;
using Recursiont.Benchmarks;

BenchmarkSwitcher.FromAssembly(typeof(TreeTraversalBenchmark).Assembly).Run(args);
