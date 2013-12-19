using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;

namespace ZambiWarz
{
    public class PriorityQueue<V, T> where V : IComparable
    {
        private int totalSize;
        private SortedDictionary<V, List<T>> storage;

        public PriorityQueue()
        {
            this.totalSize = 0;
            this.storage = new SortedDictionary<V, List<T>>();
        }

        public bool IsEmpty()
        {
            return totalSize == 0;
        }

        public T Dequeue()
        {
            if (IsEmpty())
                throw new Exception("Dequeue attempted on an empty PriorityQueue!");
            else
                foreach (List<T> q in storage.Values)
                    if (q.Count > 0)
                    {
                        --totalSize;
                        T v = q[0];
                        q.RemoveAt(0);
                        return v;
                    }

            Debug.Assert(false, "not supposed to reach here. problem with changing totalSize");

            return default(T); // not supposed to reach here.
        }

        public T Peek()
        {
            if (IsEmpty())
                throw new Exception("Peek attempted on an empty PriorityQueue!");
            else
                foreach (List<T> q in storage.Values)
                    if (q.Count > 0)
                        return q[0];

            Debug.Assert(false, "not supposed to reach here. problem with changing totalSize");

            return default(T); // not supposed to reach here.
        }

        public T Dequeue(V priority)
        {
            if (storage.ContainsKey(priority))
            {
                --totalSize;
                T v = storage[priority][0];
                storage[priority].RemoveAt(0);
                return v;
            }
            throw new Exception("No key of specified priority: " + priority);
        }

        public void Enqueue(V priority, T item)
        {
            if (!storage.ContainsKey(priority))
                storage.Add(priority, new List<T>());

            storage[priority].Add(item);
            ++totalSize;
        }

        public bool Contains(T item)
        {
            foreach (List<T> q in storage.Values)
                if (q.Contains(item))
                    return true;

            return false;
        }

        public void Remove(T item)
        {
            foreach (List<T> q in storage.Values)
                if (q.Contains(item))
                {
                    q.Remove(item);
                    return;
                }
        }
    }
}
