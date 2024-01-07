// Copyright © Theodore Tsirpanis and Contributors.
// Licensed under the MIT License (MIT).
// See LICENSE in the repository root for more information.

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace Recursiont.Infrastructure;

/// <summary>
/// Used by the compiler to implement <see langword="async"/> methods
/// that return <see cref="RecursiveOp{TResult}"/>.
/// </summary>
/// <typeparam name="TResult">The type the recursive op returns.</typeparam>
#if RECURSIONT
public
#endif
struct AsyncRecursiveOpMethodBuilder<TResult>
{
    private TResult? _result;

    private readonly RecursiveRunner _runner;

    private object? _task;

    /// <inheritdoc cref="AsyncRecursiveOpMethodBuilder{TResult}"/>
    public RecursiveOp<TResult> Task
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if (_task == AsyncRecursiveOpMethodBuilderShared.s_completionSentinel)
            {
                return RecursiveOp.FromResult(_result!);
            }
            return GetTaskSlow();
        }
    }

    private RecursiveOp<TResult> GetTaskSlow()
    {
        switch (_task)
        {
            case RecursiveTask<TResult> task: return task.AsTypedRecursiveOp();
            case ExceptionDispatchInfo edi: return RecursiveOp.FromException<TResult>(edi);
            default:
                ThrowHelpers.ThrowRecursiveOpInvalidUse();
                return default;
        }
    }

    private AsyncRecursiveOpMethodBuilder(RecursiveRunner runner)
    {
        _result = default;
        _runner = runner;
        _task = null;
    }

    /// <inheritdoc cref="AsyncRecursiveOpMethodBuilder{TResult}"/>
    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : IRecursiveCompletion where TStateMachine : IAsyncStateMachine =>
        AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);

    /// <inheritdoc cref="AsyncRecursiveOpMethodBuilder{TResult}"/>
    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : IRecursiveCompletion where TStateMachine : IAsyncStateMachine
    {
        _runner.ValidateSameRunner(awaiter.Runner);
        awaiter.UnsafeOnCompleted(GetStateMachineBox(ref stateMachine, ref _task, _runner));
    }

    /// <inheritdoc cref="AsyncRecursiveOpMethodBuilder{TResult}"/>
    public static AsyncRecursiveOpMethodBuilder<TResult> Create() =>
        new(RecursiveRunner.GetCurrentRunner());

    internal static RecursiveTask<TResult> GetStateMachineBox<TStateMachine>(ref TStateMachine stateMachine, [NotNull] ref object? task, RecursiveRunner runner)
        where TStateMachine : IAsyncStateMachine
    {
        ExecutionContext? executionContext = ExecutionContext.Capture();
        switch (task)
        {
            case null:
                StateMachineBox<TStateMachine> newBox = StateMachineBox<TStateMachine>.RentFromPool();
                newBox.BindRunner(runner);
                task = newBox;
                newBox.StateMachine = stateMachine;
                newBox.Context = executionContext;
                return newBox;
            case StateMachineBox<TStateMachine> box:
                box.Context = executionContext;
                return box;
            default:
                ThrowHelpers.ThrowRecursiveOpInvalidUse();
                return null;
        }
    }

    internal static void Start<TStateMachine>(ref TStateMachine stateMachine, ref object? task, RecursiveRunner runner)
        where TStateMachine : IAsyncStateMachine
    {
        // That's the magic of Recursion't. If we have enough stack space,
        // we can use normal recursion, until we go too deep, where we
        // create a state machine and queue it to the runner.
        if (RuntimeHelpersCompat.TryEnsureSufficientExecutionStack())
        {
            AsyncRecursiveOpMethodBuilderShared.Start(ref stateMachine);
        }
        else
        {
            RecursiontEventSource log = RecursiontEventSource.Log;
            if (log.IsEnabled())
            {
                log.RecursiveOpStackSpill<TStateMachine>();
            }
            RecursiveWorkItem workItem = GetStateMachineBox(ref stateMachine, ref task, runner);
            runner.QueueWorkItem(workItem);
        }
    }

    /// <inheritdoc cref="AsyncRecursiveOpMethodBuilder{TResult}"/>
    public void SetException(Exception exception) =>
        AsyncRecursiveOpMethodBuilderShared.SetException(ref _task, exception);

    /// <inheritdoc cref="AsyncRecursiveOpMethodBuilder{TResult}"/>
    public void SetStateMachine(IAsyncStateMachine stateMachine) =>
        ArgumentNullExceptionCompat.ThrowIfNull(stateMachine);

    /// <inheritdoc cref="AsyncRecursiveOpMethodBuilder{TResult}"/>
    public void SetResult(TResult result)
    {
        switch (_task)
        {
            case null:
                _result = result;
                _task = AsyncRecursiveOpMethodBuilderShared.s_completionSentinel;
                return;
            case RecursiveTask<TResult> task:
                task.SetResult(result);
                return;
        }

        ThrowHelpers.ThrowRecursiveOpInvalidUse();
    }

    /// <inheritdoc cref="AsyncRecursiveOpMethodBuilder{TResult}"/>
    public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine =>
        Start(ref stateMachine, ref _task, _runner);

    private sealed class StateMachineBox<TStateMachine> : RecursiveTask<TResult> where TStateMachine : IAsyncStateMachine
    {
        private static readonly ConcurrentBag<StateMachineBox<TStateMachine>> s_pool = InitializePoolAndTrimming();

        public TStateMachine? StateMachine;
        public ExecutionContext? Context;

        private static ConcurrentBag<StateMachineBox<TStateMachine>> InitializePoolAndTrimming()
        {
            ConcurrentBag<StateMachineBox<TStateMachine>> pool = new();
            Gen2GcCallback.Register(static x =>
            {
                var pool = (ConcurrentBag<StateMachineBox<TStateMachine>>)x;
                int itemsToRemove = pool.Count / 2;
                for (int i = 0; i < itemsToRemove && pool.TryTake(out _); i++) ;
            }, pool);
            return pool;
        }

        public static StateMachineBox<TStateMachine> RentFromPool()
        {
            if (s_pool.TryTake(out StateMachineBox<TStateMachine>? stateMachine))
            {
                return stateMachine;
            }

            return new();
        }

        internal override void Reset()
        {
            base.Reset();
            s_pool.Add(this);
        }

        internal override void Run()
        {
            if (Context is null)
            {
                Debug.Assert(StateMachine is not null);
                StateMachine!.MoveNext();
            }
            else
            {
                ExecutionContext.Run(Context, static x =>
                {
                    ((StateMachineBox<TStateMachine>)x!).StateMachine!.MoveNext();
                }, this);
            }

            if (IsCompleted)
            {
                StateMachine = default;
                Context = null;
            }
        }
    }
}
