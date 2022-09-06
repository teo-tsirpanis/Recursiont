// Copyright © Theodore Tsirpanis and Contributors.
// Licensed under the MIT License (MIT).
// See LICENSE in the repository root for more information.

using System.Runtime.CompilerServices;

namespace Recursiont;

internal abstract class RecursiveTask<TResult> : RecursiveTask
{
    private TResult? _result;

    internal RecursiveTask(RecursiveRunner runner) : base(runner) { }

    internal RecursiveOp<TResult> AsTypedRecursiveOp() => new(this, Token);

    internal TResult GetTypedResult(ushort token)
    {
        TResult result = _result!;
        GetResult(token);
        return result;
    }

    internal void SetResult(TResult result)
    {
        if (!TrySetCompletionObject(s_completionSentinel))
        {
            ThrowHelpers.ThrowRecursiveOpInvalidUse();
        }

        _result = result;

        InvokeContinuationIfExists();
    }

    internal override void Reset()
    {
        base.Reset();
#if !NETSTANDARD2_0
        if (RuntimeHelpers.IsReferenceOrContainsReferences<TResult>())
#endif
        {
            _result = default;
        }
    }
}
