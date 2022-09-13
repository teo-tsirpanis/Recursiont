// Copyright © Theodore Tsirpanis and Contributors.
// Licensed under the MIT License (MIT).
// See LICENSE in the repository root for more information.

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Recursiont;

/// <summary>
/// Safely runs infintely recursive functions.
/// </summary>
public partial class RecursiveRunner
{
    /// <summary>
    /// A cached per-thread <see cref="RecursiveRunner"/> object.
    /// </summary>
    [ThreadStatic]
    private static RecursiveRunner? t_cachedRunner;

    /// <summary>
    /// The <see cref="RecursiveRunner"/> object assigned to the current thread.
    /// </summary>
    /// <remarks>
    /// It is an ambient state needed by the <see langword="async"/> method builders.
    /// </remarks>
    [ThreadStatic]
    private static RecursiveRunner? t_currentRunner;

    private RecursiveWorkItem? _nextWorkItem;

    private RecursiveRunner() { }

    internal static RecursiveRunner GetCurrentRunner()
    {
        RecursiveRunner? runner = t_currentRunner;
        if (runner is null)
        {
            ThrowHelpers.ThrowNoCurrentRunner();
        }
        return runner;
    }

    private void Reset()
    {
        _nextWorkItem = null;
        t_cachedRunner ??= this;
    }

    private void Evaluate(RecursiveOp op)
    {
        try
        {
            Debug.Assert(t_currentRunner == this);

            if (!op.IsCompleted)
            {
                RunWorkItemsUntilTaskCompletes(op.UnderlyingTask);
            }

            op.GetAwaiter().GetResult();
        }
        finally
        {
            Reset();
        }
    }

    private TResult Evaluate<TResult>(RecursiveOp<TResult> op)
    {
        try
        {
            Debug.Assert(t_currentRunner == this);

            if (!op.IsCompleted)
            {
                RunWorkItemsUntilTaskCompletes(op.UnderlyingTask);
            }

            return op.GetAwaiter().GetResult();
        }
        finally
        {
            Reset();
        }
    }

    internal void QueueWorkItem(RecursiveWorkItem workItem, bool yielding = false)
    {
        Debug.Assert(workItem.Runner == this);
        if (_nextWorkItem != null)
        {
            ThrowHelpers.ThrowMustImmediatelyAwait();
        }

        // If we are running a task's continuation we might have enough
        // stack space to directly run it instead of queueing it.
        // This optimization has to be disabled when awaiting RecursiveOp.Yield().
        if (!yielding && RuntimeHelpersCompat.TryEnsureSufficientExecutionStack())
        {
            workItem.Run();
            return;
        }
        _nextWorkItem = workItem;
    }

    private static RecursiveRunner RentFromCache()
    {
        RecursiveRunner? runner = t_cachedRunner;
        if (runner is not null)
        {
            t_cachedRunner = null;
        }
        else
        {
            runner = new();
        }
        return runner;
    }

    private void RunWorkItemsUntilTaskCompletes(RecursiveTask task)
    {
        while (!task.IsCompleted && _nextWorkItem is RecursiveWorkItem nextWorkItem)
        {
            _nextWorkItem = null;
            nextWorkItem.Run();
        }
    }

    private static CurrentRunnerScope SetupRunnerFrame() => new(RentFromCache());

    internal void ValidateSameRunner(RecursiveRunner otherRunner)
    {
        if (otherRunner != this)
        {
            ThrowHelpers.ThrowMixedRunners();
        }
    }

    private readonly struct CurrentRunnerScope : IDisposable
    {
        private readonly RecursiveRunner? _previousRunner;

        public RecursiveRunner Runner { get; }

        public CurrentRunnerScope(RecursiveRunner runner)
        {
            ref RecursiveRunner? currentRunner = ref t_currentRunner;
            _previousRunner = currentRunner;
            currentRunner = Runner = runner;
        }

        public void Dispose() => t_currentRunner = _previousRunner;
    }
}
