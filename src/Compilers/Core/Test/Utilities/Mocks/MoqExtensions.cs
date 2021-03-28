// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using Moq;
using System.Linq;

namespace Loretta.Test.Utilities
{
    public static class MoqExtensions
    {
        public static void VerifyAndClear(this IInvocationList invocations, params (string Name, object[] Args)[] expectedInvocations)
        {
            AssertEx.Equal(
                expectedInvocations.Select(i => $"{i.Name}: {string.Join(",", i.Args)}"),
                invocations.Select(i => $"{i.Method.Name}: {string.Join(",", i.Arguments)}"));

            for (int i = 0; i < expectedInvocations.Length; i++)
            {
                AssertEx.Equal(expectedInvocations[i].Args, invocations[i].Arguments);
            }

            invocations.Clear();
        }
    }
}
