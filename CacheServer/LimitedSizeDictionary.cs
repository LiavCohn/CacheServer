using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CacheServer
{
    class LimitedSizeDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        const int SIZE = 16;
        public Dictionary<TKey, TValue> dict;
        Queue<TKey> queue;
        Queue<int> sizeQueue;

        public LimitedSizeDictionary()
        {
            //this.size = size;
            dict = new Dictionary<TKey, TValue>();
            queue = new Queue<TKey>();
            sizeQueue = new Queue<int>();
        }

        public void Add(TKey key, TValue value, int size)
        {
            if (size > SIZE)
                throw new Exception("Invalid size...");
            sizeQueue.Enqueue(size);
            int mySize = getSize();
            bool exceeded = isExceeded(mySize);
            while(exceeded)
            {
                dict.Remove(queue.Dequeue());// remove the oldest record
                sizeQueue.Dequeue();
                mySize = getSize();
                exceeded = isExceeded(mySize);
            }


            dict.Add(key, value);
            queue.Enqueue(key);
            
        }
        public new bool ContainsKey(TKey key)
        {
            if (dict.ContainsKey(key))
                return true;
            return false;
        }
        public new bool Remove(TKey key)
        {
            if (dict.Remove(key)) //if successfully removed
            {
                Queue<TKey> newQueue = new Queue<TKey>();
                foreach (TKey item in queue)
                    if (!dict.Comparer.Equals(item, key)) //generate a new key queue without the key that was removed
                        newQueue.Enqueue(item);
                queue = newQueue;
                return true;
            }
            else
                return false;
        }
        public int getSize()
        {
            int res = 0;
            int size = sizeQueue.Count;
            if (size > 0)
            {
                foreach (var item in sizeQueue)
                {
                    res += item;
                }
            }
            return res;
        }

        public bool isExceeded(int currSize)
        {
            return currSize > SIZE ? true : false;
        }
    }
}
