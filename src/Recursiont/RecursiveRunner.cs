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
    /// The <see cref="RecursiveRunner"/> object assigned to the current thread.
    /// </summary>
    /// <remarks>
    /// It is an ambient state needed by the <see langword="async"/> method builders.
    /// </remarks>
    [ThreadStatic]
    private static RecursiveRunner? t_currentRunner;

    private RecursiveWorkItem? _nextWorkItem;

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

    private void RunWorkItemsUntilTaskCompletes(RecursiveTask task)
    {
        while (!task.IsCompleted && _nextWorkItem is RecursiveWorkItem nextWorkItem)
        {
            _nextWorkItem = null;
            nextWorkItem.Run();
        }
    }

    private CurrentRunnerScope SetCurrentRunner() => new(this);

    private readonly struct CurrentRunnerScope : IDisposable
    {
        private readonly RecursiveRunner? _previousRunner;

        public CurrentRunnerScope(RecursiveRunner runner)
        {
            ref RecursiveRunner? currentRunner = ref t_currentRunner;
            _previousRunner = currentRunner;
            currentRunner = runner;
        }

        public void Dispose() => t_currentRunner = _previousRunner;
    }
}
