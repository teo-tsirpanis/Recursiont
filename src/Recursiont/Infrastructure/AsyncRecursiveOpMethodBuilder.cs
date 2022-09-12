// Copyright © Theodore Tsirpanis and Contributors.
// Licensed under the MIT License (MIT).
// See LICENSE in the repository root for more information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace Recursiont.Infrastructure;

/// <summary>
/// Used by the compiler to implement <see langword="async"/>
/// methods that return <see cref="RecursiveOp"/>.
/// </summary>
public struct AsyncRecursiveOpMethodBuilder
{
    private readonly RecursiveRunner _runner;

    private object? _task;

    /// <inheritdoc cref="AsyncRecursiveOpMethodBuilder"/>
    public RecursiveOp Task
    {
        get
        {
            if (_task == AsyncRecursiveOpMethodBuilderShared.s_completionSentinel)
            {
                return RecursiveOp.CompletedOp;
            }
            switch (_task)
            {
                case RecursiveTask task: return task.AsRecursiveOp();
                case ExceptionDispatchInfo edi: return RecursiveOp.FromException(edi);
                default:
                    ThrowHelpers.ThrowRecursiveOpInvalidUse();
                    return default;
            }
        }
    }

    private AsyncRecursiveOpMethodBuilder(RecursiveRunner runner)
    {
        _runner = runner;
        _task = null;
    }

    /// <inheritdoc cref="AsyncRecursiveOpMethodBuilder"/>
    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : IRecursiveCompletion where TStateMachine : IAsyncStateMachine =>
        AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);

    /// <inheritdoc cref="AsyncRecursiveOpMethodBuilder"/>
    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : IRecursiveCompletion where TStateMachine : IAsyncStateMachine
    {
        RecursiveTask stateMachineBox = AsyncRecursiveOpMethodBuilder<VoidOpResult>.GetStateMachineBox(ref stateMachine, ref _task, _runner);
        awaiter.UnsafeOnCompleted(stateMachineBox);
    }

    /// <inheritdoc cref="AsyncRecursiveOpMethodBuilder"/>
    public static AsyncRecursiveOpMethodBuilder Create() =>
        new(RecursiveRunner.GetCurrentRunner());

    /// <inheritdoc cref="AsyncRecursiveOpMethodBuilder"/>
    public void SetException(Exception exception) =>
        AsyncRecursiveOpMethodBuilderShared.SetException(ref _task, exception);

    /// <inheritdoc cref="AsyncRecursiveOpMethodBuilder"/>
    public void SetStateMachine(IAsyncStateMachine stateMachine) =>
        ArgumentNullExceptionCompat.ThrowIfNull(stateMachine);

    /// <inheritdoc cref="AsyncRecursiveOpMethodBuilder"/>
    public void SetResult()
    {
        switch (_task)
        {
            case null:
                _task = AsyncRecursiveOpMethodBuilderShared.s_completionSentinel;
                return;
            case RecursiveTask task:
                task.SetResult();
                return;
        }

        ThrowHelpers.ThrowRecursiveOpInvalidUse();
    }

    /// <inheritdoc cref="AsyncRecursiveOpMethodBuilder"/>
    public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine =>
        AsyncRecursiveOpMethodBuilder<VoidOpResult>.Start(ref stateMachine, ref _task, _runner);

    private struct VoidOpResult { }
}
