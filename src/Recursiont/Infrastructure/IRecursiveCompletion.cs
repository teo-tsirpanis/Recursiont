// Copyright © Theodore Tsirpanis and Contributors.
// Licensed under the MIT License (MIT).
// See LICENSE in the repository root for more information.

using System.Runtime.CompilerServices;

namespace Recursiont.Infrastructure;

/// <summary>
/// This interface is implemented by the awaiters of types that can be
/// <see langword="await"/>ed inside an <see langword="async"/> method
/// that returns <see cref="RecursiveOp"/> or <see cref="RecursiveOp{TResult}"/>.
/// These types are the aforementioned two.
/// </summary>
/// <remarks>
/// User code cannot implement this interface. <see langword="await"/>ing types that
/// implement it in an <see langword="async"/> method that returns any other type will fail.
/// </remarks>
public interface IRecursiveCompletion : ICriticalNotifyCompletion
{
    internal void UnsafeOnCompleted(RecursiveWorkItem workItem);
}
