using System;
using System.Collections.Generic;

namespace EcosDeLaMazmorra.UI
{
    public class FlowStack<T>
    {
        private readonly Stack<T> stack = new();
        private readonly Action<List<T>> onRemove;

        public FlowStack(Action<List<T>> onRemove = null)
        {
            this.onRemove = onRemove;
        }

        public void Push(T element)
        {
            List<T> removedItems = RemoveLoopIfExists(element);
            if (removedItems == null)
            {
                stack.Push(element);
                return;
            }
            onRemove?.Invoke(removedItems);
        }

        public T Pop()    => stack.Count > 0 ? stack.Pop()  : default;
        public T Peek()   => stack.Count > 0 ? stack.Peek() : default;
        public int Count  => stack.Count;
        public void Clear() => stack.Clear();
        public bool Contains(T item) => stack.Contains(item);

        private List<T> RemoveLoopIfExists(T target)
        {
            if (!stack.Contains(target)) return null;

            List<T> removedItems = new List<T>();
            while (stack.Count > 0 && !EqualityComparer<T>.Default.Equals(stack.Peek(), target))
                removedItems.Add(Pop());

            return removedItems;
        }

        public void PrintStack()
        {
            string text = "Current Stack: (Click for expanded view)\n";
            foreach (var item in stack)
                text += $"{item}\n";
            UnityEngine.Debug.Log(text);
        }
    }
}
