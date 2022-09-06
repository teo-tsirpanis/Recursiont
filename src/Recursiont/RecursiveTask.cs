// Copyright © Theodore Tsirpanis and Contributors.
// Licensed under the MIT License (MIT).
// See LICENSE in the repository root for more information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;

namespace Recursiont;

/// <summary>
/// The backing source for <see cref="RecursiveOp"/>s.
/// </summary>
/// <remarks>
/// This is the equivalent to the .NET's IValueTaskSource but since
/// we control the entire stack we can make it an abstract class.
/// Recursion't does not have an equivalent type for plain <see cref="Task"/>s.
/// </remarks>
internal abstract class RecursiveTask : RecursiveWorkItem
{
    protected static readonly object s_completionSentinel = new();

    // The task's state. s_completionSentinel if it succeeded
    // or an ExceptionDispatchInfo if it failed and null if it is pending.
    private object? _completionObject;

    // The work item that will run once this task completes.
    private RecursiveWorkItem? _continuation;

    protected ushort Token { get; private set; }

    [MemberNotNullWhen(true, nameof(_completionObject))]
    protected bool IsCompleted => _completionObject is not null;

    internal RecursiveTask(RecursiveRunner runner) : base(runner) { }

    internal RecursiveOp AsRecursiveOp() => new(this, Token);

    internal bool GetIsCompleted(ushort token)
    {
        ValidateToken(token);
        return IsCompleted;
    }

    internal void GetResult(ushort token)
    {
        try
        {
            ValidateToken(token);
            if (_completionObject is null)
            {
                ThrowHelpers.ThrowRecursiveOpNotCompleted();
            }

            if (_completionObject is ExceptionDispatchInfo edi)
            {
                edi.Throw();
            }
        }
        finally
        {
            Reset();
        }
    }

    protected void InvokeContinuationIfExists()
    {
        if (_continuation is RecursiveWorkItem continuation)
        {
            Runner.QueueWorkItem(continuation);
        }
    }

    internal virtual void Reset()
    {
        Token++;
        _continuation = null;
        _completionObject = null;
    }

    internal void SetException(Exception exception)
    {
        if (!TrySetCompletionObject(ExceptionDispatchInfo.Capture(exception)))
        {
            ThrowHelpers.ThrowRecursiveOpInvalidUse();
        }

        InvokeContinuationIfExists();
    }

    internal void SetResult()
    {
        if (!TrySetCompletionObject(s_completionSentinel))
        {
            ThrowHelpers.ThrowRecursiveOpInvalidUse();
        }

        InvokeContinuationIfExists();
    }

    protected bool TrySetCompletionObject(object completionObject)
    {
        Debug.Assert(completionObject == s_completionSentinel || completionObject is ExceptionDispatchInfo);
        object? previousCompletionObject = Interlocked.CompareExchange(ref _completionObject, completionObject, null);
        return previousCompletionObject is null;
    }

    internal void UnsafeOnCompleted(RecursiveWorkItem continuation, ushort token)
    {
        ValidateSameRunners(continuation);
        ValidateToken(token);
        RecursiveWorkItem? previousContinuation = Interlocked.CompareExchange(ref _continuation, continuation, null);
        if (previousContinuation is not null)
        {
            ThrowHelpers.ThrowRecursiveOpMultipleAwaits();
        }

        if (IsCompleted)
        {
            Runner.QueueWorkItem(_continuation);
        }
    }

    private void ValidateSameRunners(RecursiveWorkItem workItem)
    {
        if (workItem.Runner != Runner)
        {
            ThrowHelpers.ThrowMixedRunners();
        }
    }

    private void ValidateToken(ushort token)
    {
        if (token != Token)
        {
            ThrowHelpers.ThrowRecursiveOpMultipleAwaits();
        }
    }
}
