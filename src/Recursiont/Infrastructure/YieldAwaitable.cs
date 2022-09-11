// Copyright © Theodore Tsirpanis and Contributors.
// Licensed under the MIT License (MIT).
// See LICENSE in the repository root for more information.

using System.Runtime.CompilerServices;

namespace Recursiont.Infrastructure;

internal readonly struct YieldAwaitable
{
    private readonly RecursiveRunner _runner;

    internal YieldAwaitable(RecursiveRunner runner)
    {
        _runner = runner;
    }

    public YieldAwaiter GetAwaiter() => new(_runner);
}

internal readonly struct YieldAwaiter : IRecursiveCompletion
{
    private readonly RecursiveRunner _runner;

    public bool IsCompleted => false;

    internal YieldAwaiter(RecursiveRunner runner)
    {
        _runner = runner;
    }

    public void GetResult() { }

    RecursiveRunner IRecursiveCompletion.Runner => _runner;

    void INotifyCompletion.OnCompleted(Action continuation) =>
        ThrowHelpers.ThrowCannotAwaitOutsideRecursiveFunction();

    void ICriticalNotifyCompletion.UnsafeOnCompleted(Action continuation) =>
        ThrowHelpers.ThrowCannotAwaitOutsideRecursiveFunction();

    void IRecursiveCompletion.UnsafeOnCompleted(RecursiveWorkItem workItem) =>
        _runner.QueueWorkItem(workItem, yielding: true);
}
