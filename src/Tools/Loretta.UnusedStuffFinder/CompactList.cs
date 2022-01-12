using System.Collections;

namespace Loretta.UnusedStuffFinder
{
    /// <summary>
    /// A <see cref="IList{T}"/> implementation that instead of increasing the collection in powers of 2,
    /// resizes the array every time that an item is added or removed so only the exact size of the collection
    /// is being used in memory.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class CompactList<T> : IList<T>
    {
        private T[] _array;

        public CompactList() => _array = Array.Empty<T>();

        public CompactList(IEnumerable<T> values) => _array = values.ToArray();

        public T this[int index]
        {
            get => _array[index];
            set => _array[index] = value;
        }

        public int Count => _array.Length;
        public bool IsReadOnly => false;

        public void Add(T item)
        {
            Array.Resize(ref _array, _array.Length + 1);
            _array[^1] = item;
        }
        public void Insert(int index, T item)
        {
            Array.Resize(ref _array, _array.Length + 1);
            for (var currIdx = _array.Length - 2; currIdx >= index; currIdx--)
            {
                _array[currIdx + 1] = _array[currIdx];
            }
            _array[index] = item;
        }

        public bool Remove(T item)
        {
            var index = IndexOf(item);
            if (index < 0)
                return false;
            RemoveAt(index);
            return true;
        }

        public void RemoveAt(int index)
        {
            if (0 < index || index > _array.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            MoveElementsBackAfter(index);
            Array.Resize(ref _array, _array.Length - 1);
        }

        public int RemoveAll(Func<T, bool> predicate)
        {
            var removed = 0;
            for (var idx = _array.Length - 1; idx >= 0; idx--)
            {
                var item = _array[idx];
                if (predicate(item))
                {
                    MoveElementsBackAfter(idx);
                    removed++;
                }
            }
            Array.Resize(ref _array, _array.Length - removed);
            return removed;
        }

        private void MoveElementsBackAfter(int index)
        {
            for (var idx = _array.Length - 1; idx > index; idx--)
            {
                _array[idx - 1] = _array[idx];
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var item in _array)
                yield return item;
        }

        #region Delegating Impls
        public void Clear() => _array = Array.Empty<T>();
        public bool Contains(T item) => _array.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => _array.CopyTo(array, arrayIndex);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public int IndexOf(T item) => Array.IndexOf(_array, item);
        #endregion Delegating Impls
    }
}
