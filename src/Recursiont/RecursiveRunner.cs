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
    private readonly Stack<RecursiveWorkItem> _workItemsToRun = new();

    internal void QueueWorkItem(RecursiveWorkItem workItem)
    {
        Debug.Assert(workItem.Runner == this);
        _workItemsToRun.Push(workItem);
    }
}
