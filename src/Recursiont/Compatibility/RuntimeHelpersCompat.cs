// Copyright © Theodore Tsirpanis and Contributors.
// Licensed under the MIT License (MIT).
// See LICENSE in the repository root for more information.

#if NETSTANDARD2_0
using System.Runtime.CompilerServices;

internal static class RuntimeHelpersCompat
{
    public static bool TryEnsureSufficientExecutionStack()
    {
        try
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();
            return true;
        }
        catch (InsufficientExecutionStackException)
        {
            return false;
        }
    }
}
#else
global using RuntimeHelpersCompat = System.Runtime.CompilerServices.RuntimeHelpers;
#endif
