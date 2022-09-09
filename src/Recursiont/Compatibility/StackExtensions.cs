// Copyright © Theodore Tsirpanis and Contributors.
// Licensed under the MIT License (MIT).
// See LICENSE in the repository root for more information.

using System.Diagnostics.CodeAnalysis;

namespace System.Collections.Generic;

internal static class StackExtensions
{
    public static bool TryPop<T>(this Stack<T> stack, [MaybeNullWhen(false)] out T value)
    {
        if (stack.Count > 0)
        {
            value = stack.Pop();
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }
}
