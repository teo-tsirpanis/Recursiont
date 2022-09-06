// Copyright © Theodore Tsirpanis and Contributors.
// Licensed under the MIT License (MIT).
// See LICENSE in the repository root for more information.

using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace Recursiont;

/// <summary>
/// A <see cref="RecursiveOp"/> that gives a value when <see langword="await"/>ed.
/// </summary>
/// <typeparam name="TResult">The type of the value that will be returned.</typeparam>
/// <remarks>
/// This type shares the same samantics with <see cref="RecursiveOp"/>.
/// </remarks>
/// <seealso cref="RecursiveOp"/>
public readonly struct RecursiveOp<TResult>
{
    // Can be null, an ExceptionDispatchInfo, or a RecursiveTask<TResult>.
    internal readonly TResult? _result;
    internal readonly object? _taskOrEdi;
    internal readonly ushort _token;

    internal bool IsCompleted
    {
        get
        {
            object? taskOrEdi = _taskOrEdi;
            Debug.Assert(taskOrEdi is null or ExceptionDispatchInfo or RecursiveTask<TResult>);

            if (taskOrEdi is RecursiveTask<TResult> task)
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
            Debug.Assert(_taskOrEdi is RecursiveTask<TResult>);
            return ((RecursiveTask<TResult>)_taskOrEdi!).Runner;
        }
    }

    internal RecursiveOp(TResult result)
    {
        _result = result;
        _taskOrEdi = null;
        _token = 0;
    }

    internal RecursiveOp(object taskOrEdi, ushort token)
    {
        Debug.Assert(taskOrEdi is RecursiveTask<TResult> or ExceptionDispatchInfo);
        _result = default;
        _taskOrEdi = taskOrEdi;
        _token = token;
    }

    /// <summary>
    /// Used by the compiler to implement the <see langword="await"/> pattern.
    /// </summary>
    public RecursiveOpAwaiter<TResult> GetAwaiter() => new(this);

    internal TResult GetResult()
    {
        switch (_taskOrEdi)
        {
            case null: return _result!;
            case RecursiveTask<TResult> task: return task.GetTypedResult(_token)!;
            case ExceptionDispatchInfo edi: edi.Throw(); break;
        }
        Debug.Assert(false);
        return default!;
    }

    internal void UnsafeOnCompleted(RecursiveWorkItem workItem)
    {
        if (_taskOrEdi is RecursiveTask<TResult> task)
        {
            task.UnsafeOnCompleted(workItem, _token);
            return;
        }

        // If the op does not have a RecursiveTask, it has
        // completed and we should not have called this function.
        ThrowHelpers.ThrowRecursiveOpInvalidUse();
    }
}
