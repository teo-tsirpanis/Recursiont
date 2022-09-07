// Copyright © Theodore Tsirpanis and Contributors.
// Licensed under the MIT License (MIT).
// See LICENSE in the repository root for more information.

using System.Diagnostics;

namespace Recursiont;

/// <summary>
/// Safely runs infintely recursive functions.
/// </summary>
public class RecursiveRunner
{
    /// <summary>
    /// The <see cref="RecursiveRunner"/> object assigned to the current thread.
    /// </summary>
    /// <remarks>
    /// It is an ambient state needed by the <see langword="async"/> method builders.
    /// </remarks>
    [ThreadStatic]
    private static RecursiveRunner? t_currentRunner;

    private readonly Stack<RecursiveWorkItem> _workItemsToRun = new();

    internal static RecursiveRunner GetCurrentRunner()
    {
        RecursiveRunner? runner = t_currentRunner;
        if (runner is null)
        {
            ThrowHelpers.ThrowNoCurrentRunner();
        }
        return runner;
    }

    internal void QueueWorkItem(RecursiveWorkItem workItem)
    {
        Debug.Assert(workItem.Runner == this);
        _workItemsToRun.Push(workItem);
    }

    private CurrentRunnerScope SetCurrentRunner() => new(this);

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

        public CurrentRunnerScope(RecursiveRunner runner)
        {
            ref RecursiveRunner? currentRunner = ref t_currentRunner;
            _previousRunner = currentRunner;
            currentRunner = runner;
        }

        public void Dispose() => t_currentRunner = _previousRunner;
    }
}
