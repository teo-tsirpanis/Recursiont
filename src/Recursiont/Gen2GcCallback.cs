// Copyright © Theodore Tsirpanis and Contributors.
// Licensed under the MIT License (MIT).
// See LICENSE in the repository root for more information.

using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;

namespace Recursiont;

// Adopted from the .NET BCL, cut down to Recursion't's needs.
internal sealed class Gen2GcCallback : CriticalFinalizerObject
{
    private readonly Action<object> _callback;

    private GCHandle _state;

    private Gen2GcCallback(Action<object> callback, object state)
    {
        _callback = callback;
        _state = GCHandle.Alloc(state, GCHandleType.Weak);
    }

    public static void Register(Action<object> callback, object state)
    {
        _ = new Gen2GcCallback(callback, state);
    }

    ~Gen2GcCallback()
    {
        if (_state.Target is not object state)
        {
            _state.Free();
            return;
        }

        _callback(state);
        GC.ReRegisterForFinalize(this);
    }
}
