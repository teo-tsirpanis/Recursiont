// Copyright © Theodore Tsirpanis and Contributors.
// Licensed under the MIT License (MIT).
// See LICENSE in the repository root for more information.

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

namespace Recursiont.Tests;

internal class UnloadabilityTests
{
    [Test]
    public void RecursiveTaskPoolDoesNotInhibitUnloadability()
    {
        WeakReference alcWeakRef = CreateALC();
        int i;
        for (i = 0; i < 10 && alcWeakRef.IsAlive; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        Assert.That(alcWeakRef.IsAlive, Is.False, $"Cannot unload assembly after {i} garbage collections.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        static WeakReference CreateALC()
        {
            TestAssemblyLoadContext alc = new();
            Assembly asm = alc.LoadFromAssemblyPath(typeof(UnloadabilityTests).Assembly.Location);
            asm
                .GetType(typeof(UnloadabilityTests).FullName!, true)!
                .GetMethod(nameof(TestRecursiveMethod), BindingFlags.Static | BindingFlags.NonPublic)!
                .Invoke(null, null);

            return new WeakReference(alc, trackResurrection: true);
        }
    }

    private static void TestRecursiveMethod()
    {
        RecursiveRunner.Run(async () => await RecursiveOp.Yield());
    }

    private sealed class TestAssemblyLoadContext : AssemblyLoadContext
    {
        public TestAssemblyLoadContext() : base(isCollectible: true)
        {
        }

        protected override Assembly? Load(AssemblyName assemblyName) => assemblyName.Name switch
        {
            "Recursiont" => typeof(RecursiveOp).Assembly,
            _ => null,
        };
    }
}
