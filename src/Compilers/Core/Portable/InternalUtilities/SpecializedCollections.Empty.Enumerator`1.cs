// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Loretta.Utilities
{
    internal partial class SpecializedCollections
    {
        private partial class Empty
        {
            internal class Enumerator<T> : Enumerator, IEnumerator<T>
            {
                public static new readonly IEnumerator<T> Instance = new Enumerator<T>();

                protected Enumerator()
                {
                }

                public new T Current => throw new InvalidOperationException();

                public void Dispose()
                {
                }
            }
        }
    }
}
