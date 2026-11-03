using System;
using System.Collections.Generic;
using System.Linq;

namespace FrontierDispatch.Core
{
    // Assignment Phase 3. Implement as a binary min-heap.

    /// <summary>
    /// A minimum-priority queue implemented as a binary min-heap.
    ///
    /// Items with lower integer priority values should be returned before
    /// items with higher priority values.
    ///
    /// This class is intentionally incomplete in the assignment starter.
    /// </summary>
    public sealed class MinPriorityQueue<T>
    {
        // The list stores the physical representation of the binary heap.
        // Your implementation is responsible for preserving the min-heap invariant.
        private readonly List<Entry> _heap = new List<Entry>();

        /// <summary>
        /// Number of items currently stored in the queue.
        /// </summary>
        public int Count => _heap.Count;

        #region ENQUEUE / DEQUEUE
        /// <summary>
        /// Adds an item with the supplied priority.
        /// Lower priority values are removed before higher priority values.
        /// </summary>
        public void Enqueue(T item, int priority)
        {
            Entry newEntry = new Entry(item, priority);
            _heap.Add(newEntry);

            MoveUp(_heap.Count - 1);
        }
        
         /// <summary>
         /// Removes and returns the minimum-priority item.
         /// </summary>
         /// <exception cref="InvalidOperationException">
         /// Thrown when the queue is empty.
         /// </exception>
        public T Dequeue()
        {
            if (_heap.Count == 0) throw new InvalidOperationException("The queue is empty");

            T rootItem = _heap.First().Item;
            int lastIndex = _heap.Count - 1;

            if (lastIndex > 0)
                _heap[0] = _heap[lastIndex];

            _heap.RemoveAt(lastIndex);

            if (_heap.Count > 0)
                MoveDown(0);

            return rootItem;
        }
        #endregion

        /// <summary>
        /// Returns the minimum-priority item without removing it.
        /// </summary>
        /// 
        /// <exception cref="InvalidOperationException">
        /// Thrown when the queue is empty.
        /// </exception>
        public T Peek()
        {
            if (_heap.Count == 0) throw new InvalidOperationException("The queue is empty");
            return _heap[0].Item;
        }

        #region QUEUE ORDER MOVEMENT
        /// <summary>
        /// Moves the item with Larger priority Up the heap
        /// </summary>
        private void MoveUp(int index)
        {
            while (index > 0)
            {
                int parentIndex = (index - 1) / 2;

                #region Given Smaller Prio Check
                bool givenHasLargerPriority = _heap[index].Priority.CompareTo(_heap[parentIndex].Priority) >= 0;
                #endregion

                if (givenHasLargerPriority)
                    break;

                SwapItems(index, parentIndex);
                index = parentIndex;
            }
        }

        /// <summary>
        /// Moves the item with Smaller priority Down the heap
        /// </summary>
        private void MoveDown(int index)
        {
            int lastIndex = _heap.Count - 1;
            while (true)
            {
                int leftChildIndex = (index * 2) + 1;
                int rightChildIndex = (index * 2) + 2;
                int smallest = index;

                #region Left Smaller Prio Check
                bool leftHasSmallerPriority = (leftChildIndex <= lastIndex && 
                                                _heap[leftChildIndex].Priority.CompareTo(_heap[smallest].Priority) < 0);
                #endregion

                if (leftHasSmallerPriority)
                    smallest = leftChildIndex;

                #region Right Smaller Prio Check
                bool rightHasSmallerPriority = (rightChildIndex <= lastIndex && 
                                                _heap[rightChildIndex].Priority.CompareTo(_heap[smallest].Priority) < 0);
                #endregion

                if (rightHasSmallerPriority)
                    smallest = rightChildIndex;

                #region Given Smaller Prio Check
                bool givenHasSmallerPriority = (smallest == index);
                #endregion

                if (givenHasSmallerPriority)
                    break;
                
                SwapItems(index, smallest);
                index = smallest;
            }
        }
        #endregion

        /// <summary>
        /// Swaps two items by two given indices within the heap
        /// </summary>
        private void SwapItems(int currentIndex, int newIndex)
        {
            Entry tempEntry = _heap[currentIndex];
            _heap[currentIndex] = _heap[newIndex];
            _heap[newIndex] = tempEntry;
        }

        #region HEAP ENTRY STRUCTURE
        /// <summary>
        /// Stores one item together with the priority used by the heap.
        /// </summary>
        private readonly struct Entry
        {
            public Entry(T item, int priority)
            {
                Item = item;
                Priority = priority;
            }
            
            public T Item { get; }
            public int Priority { get; }
        }
        #endregion
    }
}
