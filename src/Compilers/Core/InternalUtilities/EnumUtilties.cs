// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Loretta.Utilities
{
    internal static class EnumUtilities
    {
        internal static T[] GetValues<T>() where T : struct => (T[]) Enum.GetValues(typeof(T));

#if DEBUG
        internal static bool ContainsAllValues<T>(int mask) where T : struct, Enum, IConvertible
        {
            foreach (var value in GetValues<T>())
            {
                var val = value.ToInt32(null);
                if ((val & mask) != val)
                {
                    return false;
                }
            }
            return true;
        }
#endif
    }
}
