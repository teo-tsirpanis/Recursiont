// Copyright © Theodore Tsirpanis and Contributors.
// Licensed under the MIT License (MIT).
// See LICENSE in the repository root for more information.

using System.Diagnostics;

namespace Recursiont;

internal abstract class RecursiveWorkItem
{
    private RecursiveRunner? _runner;

    public RecursiveRunner Runner
    {
        get
        {
            Debug.Assert(_runner is not null);
            return _runner!;
        }
    }

    internal void BindRunner(RecursiveRunner runner)
    {
        Debug.Assert(_runner is null);
        _runner = runner;
    }

    internal virtual void Reset()
    {
        _runner = null;
    }

    internal abstract void Run();
}
