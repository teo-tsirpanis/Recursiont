// Copyright © Theodore Tsirpanis and Contributors.
// Licensed under the MIT License (MIT).
// See LICENSE in the repository root for more information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace Recursiont.Infrastructure;

internal static class AsyncRecursiveOpMethodBuilderShared
{
    internal static readonly object s_completionSentinel = new();

    internal static void SetException(ref object? task, Exception exception)
    {
        switch (task)
        {
            case null:
                task = ExceptionDispatchInfo.Capture(exception);
                return;
            case RecursiveTask rtask:
                rtask.SetException(exception);
                return;
        }
        ThrowHelpers.ThrowRecursiveOpInvalidUse();
    }
}
