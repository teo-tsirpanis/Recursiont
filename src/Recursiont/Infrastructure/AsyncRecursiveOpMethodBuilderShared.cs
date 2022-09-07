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

    internal static void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine =>
         // It does some ExecutionContext trickery that can't be done from
         // public APIs. Let's just hope the method remains stateless.
         new AsyncTaskMethodBuilder().Start(ref stateMachine);
}
