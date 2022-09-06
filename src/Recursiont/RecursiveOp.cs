// Copyright © Theodore Tsirpanis and Contributors.
// Licensed under the MIT License (MIT).
// See LICENSE in the repository root for more information.

using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace Recursiont;

/// <summary>
/// Represents a recursive operation that can be <see langword="await"/>ed.
/// </summary>
/// <remarks>
/// There are some usage constraints around using recursive ops. Because they are
/// pooled to reduce allocations, they must be <see langword="await"/>ed only
/// once, and because recursion may be performed inline, they should be
/// <see langword="await"/>ed immediately.
/// </remarks>
/// <seealso cref="RecursiveOp{TResult}"/>
public readonly struct RecursiveOp
{
    // Can be null, an ExceptionDispatchInfo, or a RecursiveTask.
    internal readonly object? _taskOrEdi;
    internal readonly ushort _token;

    /// <summary>
    /// A completed <see cref="RecursiveOp"/>.
    /// </summary>
    public static RecursiveOp CompletedOp => default;

    internal bool IsCompleted
    {
        get
        {
            object? taskOrEdi = _taskOrEdi;
            Debug.Assert(taskOrEdi is null or ExceptionDispatchInfo or RecursiveTask);

            if (taskOrEdi is RecursiveTask task)
            {
                return task.GetIsCompleted(_token);
            }
            return true;
        }
    }

    internal RecursiveRunner Runner
    {
        get
        {
            // Should not be called on completed ops.
            Debug.Assert(_taskOrEdi is RecursiveTask);
            return ((RecursiveTask)_taskOrEdi!).Runner;
        }
    }

    internal RecursiveOp(object taskOrEdi, ushort token)
    {
        Debug.Assert(taskOrEdi is RecursiveTask or ExceptionDispatchInfo);
        _taskOrEdi = taskOrEdi;
        _token = token;
    }

    /// <summary>
    /// Creates a <see cref="RecursiveOp"/> that has completed with the specified <see cref="Exception"/>.
    /// </summary>
    /// <param name="exception">The exception to be thrown.</param>
    /// <returns>The faulted recursive op.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/>.</exception>
    public static RecursiveOp FromException(Exception exception)
    {
        ArgumentNullExceptionCompat.ThrowIfNull(exception);
        return new(ExceptionDispatchInfo.Capture(exception), 0);
    }

    /// <summary>
    /// Creates a <see cref="RecursiveOp{TResult}"/> that has completed with the specified <see cref="Exception"/>.
    /// </summary>
    /// <param name="exception">The exception to be thrown.</param>
    /// <returns>The faulted recursive op.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/>.</exception>
    public static RecursiveOp<TResult> FromException<TResult>(Exception exception)
    {
        ArgumentNullExceptionCompat.ThrowIfNull(exception);
        return new(ExceptionDispatchInfo.Capture(exception), 0);
    }

    /// <summary>
    /// Creates a <see cref="RecursiveOp{TResult}"/> that has completed
    /// and will return the specified value when <see langword="await"/>ed.
    /// </summary>
    /// <param name="result">The value to be returned.</param>
    /// <typeparam name="TResult">The type of the value that will be returned.</typeparam>
    /// <returns>The successful recursive op.</returns>
    public static RecursiveOp<TResult> FromResult<TResult>(TResult result) => new(result);

    /// <summary>
    /// Used by the compiler to implement the <see langword="await"/> pattern.
    /// </summary>
    public RecursiveOpAwaiter GetAwaiter() => new(this);

    internal void GetResult()
    {
        switch (_taskOrEdi)
        {
            case null: return;
            case RecursiveTask task: task.GetResult(_token); return;
            case ExceptionDispatchInfo edi: edi.Throw(); break;
        }
        Debug.Assert(false);
    }

    internal void UnsafeOnCompleted(RecursiveWorkItem workItem)
    {
        if (_taskOrEdi is RecursiveTask task)
        {
            task.UnsafeOnCompleted(workItem, _token);
            return;
        }

        // If the op does not have a RecursiveTask, it has
        // completed and we should not have called this function.
        ThrowHelpers.ThrowRecursiveOpInvalidUse();
    }
}
