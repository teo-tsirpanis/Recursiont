// Copyright © Theodore Tsirpanis and Contributors.
// Licensed under the MIT License (MIT).
// See LICENSE in the repository root for more information.

#if !NET6_0_OR_GREATER
namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class CallerArgumentExpressionAttribute : Attribute
{
    public CallerArgumentExpressionAttribute(string parameterName) { }
}
#endif
