// Copyright © Theodore Tsirpanis and Contributors.
// Licensed under the MIT License (MIT).
// See LICENSE in the repository root for more information.

using System.Diagnostics.Tracing;

namespace Recursiont;

[EventSource(Name = "Recursiont")]
internal sealed class RecursiontEventSource : EventSource
{
    public static readonly RecursiontEventSource Log = new();

    [Event(1, Level = EventLevel.Verbose, Message = "Recursive operation started with state machine type '{0}'.",
        Keywords = Keywords.Lifetime)]
    public void RecursiveOpStart()
    {
        WriteEvent(1);
    }

    [Event(2, Level = EventLevel.Verbose, Message = "Recursive operation stopped.",
        Keywords = Keywords.Lifetime)]
    public void RecursiveOpStop()
    {
        WriteEvent(2);
    }

    [Event(3, Level = EventLevel.Informational, Message = "Insufficient stack space when running recursive operation"
        + "with state machine type '{0}'. Spilling stack to heap.", Keywords = Keywords.StackSpill)]
    public void RecursiveOpStackSpill(string? StateMachineTypeName)
    {
        WriteEvent(3, StateMachineTypeName);
    }

    [NonEvent]
    public void RecursiveOpStackSpill<TStateMachine>()
    {
        RecursiveOpStackSpill(typeof(TStateMachine).FullName!);
    }

    public static class Keywords
    {
        public const EventKeywords Lifetime = (EventKeywords)0x0001;
        public const EventKeywords StackSpill = (EventKeywords)0x0002;
    }
}
