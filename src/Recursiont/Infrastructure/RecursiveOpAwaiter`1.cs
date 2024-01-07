// Copyright © Theodore Tsirpanis and Contributors.
// Licensed under the MIT License (MIT).
// See LICENSE in the repository root for more information.

namespace Recursiont.Infrastructure;

using Recursiont;
using System.Runtime.CompilerServices;

/// <summary>
/// Used by the compiler to implement the <see langword="await"/>
/// pattern for <see cref="RecursiveOp{TResult}"/>.
/// </summary>
/// <typeparam name="TResult">The type the recursive op returns.</typeparam>
#if RECURSIONT
public
#endif
readonly struct RecursiveOpAwaiter<TResult> : IRecursiveCompletion
{
    private readonly RecursiveOp<TResult> _op;

    internal RecursiveOpAwaiter(RecursiveOp<TResult> op) => _op = op;

    /// <inheritdoc cref="RecursiveOpAwaiter{TResult}"/>
    public bool IsCompleted => _op.IsCompleted;

    /// <inheritdoc cref="RecursiveOpAwaiter{TResult}"/>
    public TResult GetResult() => _op.GetResult();

    void INotifyCompletion.OnCompleted(Action continuation) =>
        ThrowHelpers.ThrowCannotAwaitOutsideRecursiveFunction();

    void ICriticalNotifyCompletion.UnsafeOnCompleted(Action continuation) =>
        ThrowHelpers.ThrowCannotAwaitOutsideRecursiveFunction();

    RecursiveRunner IRecursiveCompletion.Runner => _op.Runner;

    void IRecursiveCompletion.UnsafeOnCompleted(RecursiveWorkItem workItem) =>
        _op.UnsafeOnCompleted(workItem);
}
