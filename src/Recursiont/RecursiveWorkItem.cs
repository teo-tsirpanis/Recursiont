// Copyright © Theodore Tsirpanis and Contributors.
// Licensed under the MIT License (MIT).
// See LICENSE in the repository root for more information.

namespace Recursiont;

internal abstract class RecursiveWorkItem
{
    public RecursiveRunner Runner { get; }

    internal RecursiveWorkItem(RecursiveRunner runner)
    {
        Runner = runner;
    }

    internal abstract void Run();
}
