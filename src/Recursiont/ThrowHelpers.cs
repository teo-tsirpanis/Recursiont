// Copyright © Theodore Tsirpanis and Contributors.
// Licensed under the MIT License (MIT).
// See LICENSE in the repository root for more information.

using System.Diagnostics.CodeAnalysis;

namespace Recursiont;

internal static class ThrowHelpers
{
    [DoesNotReturn]
    public static void ThrowCannotAwaitOutsideRecursiveFunction() =>
        throw new NotSupportedException("RecursiveOps can be awaited only inside functions that return RecursiveOps.");

    [DoesNotReturn]
    public static void ThrowMixedRunners() =>
        throw new InvalidOperationException("Cannot mix RecursiveOps originating from multiple RecursiveRunners. This error likely originated due to RecursiveOps escaping the scope of a method, which is not supported.");

    [DoesNotReturn]
    public static void ThrowRecursiveOpMultipleAwaits() =>
        throw new InvalidOperationException("The RecursiveOp was tried to be used in an invalid way, likely originating due to multiple awaits, which are not allowed.");

    [DoesNotReturn]
    public static void ThrowRecursiveOpInvalidUse() =>
        throw new InvalidOperationException("The RecursiveOp was tried to be used in an invalid way, likely originating by using APIs intended for the compiler. If you see them in normal use, please open an issue in https://github.com/teo-tsirpanis/Recursiont");

    [DoesNotReturn]
    public static void ThrowRecursiveOpNotCompleted() =>
        throw new InvalidOperationException("The RecursiveOp has not yet completed. This error likely originated due to manually calling \"GetAwaiter().GetResult()\" which is not supported.");
}
