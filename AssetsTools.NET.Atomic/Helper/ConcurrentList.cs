using System;
using System.Collections;
using System.Collections.Generic;

namespace AssetsTools.NET.Atomic.Helper
{
    public class ConcurrentList<T> : IList<T>, IList
    {
        private readonly List<T> list;
        private readonly object locker = new object();

        public ConcurrentList()
        {
            list = new List<T>();
        }

        public ConcurrentList(int capacity)
        {
            list = new List<T>(capacity);
        }

        public ConcurrentList(IEnumerable<T> collection)
        {
            list = new List<T>(collection);
        }

        public T this[int index]
        {
            get
            {
                lock (locker)
                {
                    return list[index];
                }
            }
            set
            {
                lock (locker)
                {
                    list[index] = value;
                }
            }
        }

        public void Add(T item)
        {
            lock (locker)
            {
                list.Add(item);
            }
        }

        public void Insert(int index, T item)
        {
            lock (locker)
            {
                list.Insert(index, item);
            }
        }

        public bool Contains(T item)
        {
            lock (locker)
            {
                return list.Contains(item);
            }
        }

        public int IndexOf(T item)
        {
            lock (locker)
            {
                return list.IndexOf(item);
            }
        }

        public int FindIndex(Predicate<T> match)
        {
            lock (locker)
            {
                return list.FindIndex(match);
            }
        }

        public int FindIndex(int startIndex, Predicate<T> match)
        {
            lock (locker)
            {
                return list.FindIndex(startIndex, match);
            }
        }

        public int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            lock (locker)
            {
                return list.FindIndex(startIndex, count, match);
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (locker)
            {
                list.CopyTo(array, arrayIndex);
            }
        }

        public void CopyTo(Array array, int index)
        {
            CopyTo((T[]) array, index);
        }

        public List<T> ToList()
        {
            List<T> newList = new List<T>();

            lock (locker)
            {
                list.ForEach(x => newList.Add(x));
            }

            return newList;
        }

        public bool Remove(T item)
        {
            lock (locker)
            {
                return list.Contains(item);
            }
        }

        public void RemoveAt(int index)
        {
            lock (locker)
            {
                list.RemoveAt(index);
            }
        }

        public void Clear()
        {
            lock (locker)
            {
                list.Clear();
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        public int Count {
            get
            {
                lock (locker)
                {
                    return list.Count;
                }
            }
        }

        public bool IsFixedSize => false;
        public bool IsReadOnly => false;
        public bool IsSynchronized => true;
        public object SyncRoot => locker;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        object IList.this[int index]
        {
            get => this[index];
            set => this[index] = (T) value;
        }

        int IList.Add(object value)
        {
            lock (locker)
            {
                list.Add((T) value);
                return list.Count - 1;
            }
        }

        bool IList.Contains(object value)
        {
            return Contains((T)value);
        }

        void IList.Insert(int index, object value)
        {
            Insert(index, (T) value);
        }

        int IList.IndexOf(object value)
        {
            return IndexOf((T)value);
        }

        void IList.Remove(object value)
        {
            Remove((T)value);
        }
    }
}
