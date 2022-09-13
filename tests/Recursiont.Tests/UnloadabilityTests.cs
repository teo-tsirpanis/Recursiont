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

        if (alcWeakRef.IsAlive)
        {
            Assert.Fail($"Cannot unload assembly after {i} garbage collections.");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static WeakReference CreateALC()
        {
            AssemblyLoadContext alc = new(typeof(UnloadabilityTests).FullName, true);
            Assembly asm = alc.LoadFromAssemblyPath(typeof(UnloadabilityTests).Assembly.Location);
            asm
                .GetType(typeof(UnloadabilityTests).FullName!, true)!
                .GetMethod(nameof(TestRecursiveMethod), BindingFlags.Static | BindingFlags.NonPublic)!
                .Invoke(null, null);

            return new WeakReference(alc, true);
        }
    }

    private static void TestRecursiveMethod()
    {
        new RecursiveRunner().Run(async () => await RecursiveOp.Yield());
    }
}
