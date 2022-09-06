// Copyright © Theodore Tsirpanis and Contributors.
// Licensed under the MIT License (MIT).
// See LICENSE in the repository root for more information.

namespace Recursiont.Infrastructure;

using Recursiont;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

/// <summary>
/// Used by the compiler to implement the <see langword="await"/>
/// pattern for <see cref="RecursiveOp"/>.
/// </summary>
public readonly struct RecursiveOpAwaiter : IRecursiveCompletion
{
    private readonly RecursiveOp _op;

    internal RecursiveOpAwaiter(RecursiveOp op) => _op = op;

    /// <inheritdoc cref="RecursiveOpAwaiter"/>
    public bool IsCompleted => _op.IsCompleted;

    /// <inheritdoc cref="RecursiveOpAwaiter"/>
    public void GetResult() => _op.GetResult();

    void INotifyCompletion.OnCompleted(Action continuation) =>
        ThrowHelpers.ThrowCannotAwaitOutsideRecursiveFunction();

    void ICriticalNotifyCompletion.UnsafeOnCompleted(Action continuation) =>
        ThrowHelpers.ThrowCannotAwaitOutsideRecursiveFunction();

    RecursiveRunner IRecursiveCompletion.Runner => _op.Runner;

    void IRecursiveCompletion.UnsafeOnCompleted(RecursiveWorkItem workItem) =>
        _op.UnsafeOnCompleted(workItem);
}
