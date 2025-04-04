// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Loretta.CodeAnalysis
{
    internal partial class IdentifierCollection
    {
        private abstract class CollectionBase(IdentifierCollection identifierCollection) : ICollection<string>
        {
            protected readonly IdentifierCollection IdentifierCollection = identifierCollection;

            public abstract bool Contains(string item);

            public void CopyTo(string[] array, int arrayIndex)
            {
                using var enumerator = GetEnumerator();
                while (arrayIndex < array.Length && enumerator.MoveNext())
                {
                    // ReSharper disable once RedundantSuppressNullableWarningExpression // only happens in .NET Standard
                    array[arrayIndex] = enumerator.Current!;
                    arrayIndex++;
                }
            }

            public int Count
            {
                get
                {
                    if (field == -1)
                    {
                        field = IdentifierCollection._map.Values.Sum(
                            static o => o is string ? 1 : ((ISet<string>) o).Count);
                    }

                    return field;
                }
            } = -1;

            public bool IsReadOnly => true;

            public IEnumerator<string> GetEnumerator()
            {
                foreach (var obj in IdentifierCollection._map.Values)
                {
                    if (obj is HashSet<string> strSet)
                    {
                        foreach (var str in strSet)
                        {
                            yield return str;
                        }
                    }
                    else
                    {
                        yield return (string) obj;
                    }
                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

            #region Unsupported

            public void Add(string item) => throw new NotSupportedException();

            public void Clear() => throw new NotSupportedException();

            public bool Remove(string item) => throw new NotSupportedException();

            #endregion
        }

        private sealed class CaseSensitiveCollection(IdentifierCollection identifierCollection) : CollectionBase(
            identifierCollection)
        {
            public override bool Contains(string item) => IdentifierCollection.CaseSensitiveContains(item);
        }

        private sealed class CaseInsensitiveCollection(IdentifierCollection identifierCollection) : CollectionBase(
            identifierCollection)
        {
            public override bool Contains(string item) => IdentifierCollection.CaseInsensitiveContains(item);
        }
    }
}
