using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kosson.KRUD
{
	class LRU<TKey, TValue>
	{
		private readonly int maxSize;
		private readonly Dictionary<TKey, Node> lookup;
		private Node head;
		private Node tail;
		private object syncroot;

		public LRU(int maxSize)
		{
			this.maxSize = maxSize;
			lookup = new Dictionary<TKey, Node>(maxSize);
			syncroot = new object();
		}

		public TValue this[TKey key]
		{
			get
			{
				lock (syncroot)
				{
					Node node;
					if (!lookup.TryGetValue(key, out node)) return default(TValue);
					MoveToFront(node);
					return node.Value;
				}
			}

			set
			{
				lock (syncroot)
				{
					Node node;
					if (!lookup.TryGetValue(key, out node))
					{
						Reduce();
						node = new Node();
						node.Key = key;
						lookup[key] = node;
					}
					node.Value = value;
					MoveToFront(node);
				}
			}
		}

		private void MoveToFront(Node newHead)
		{
			if (newHead == head) return;
			if (head == null)
			{
				head = newHead;
				tail = newHead;
				return;
			}

			if (newHead == tail) tail = newHead.Prev;

			if (newHead.Prev != null) newHead.Prev.Next = newHead.Next;

			newHead.Next = head;
			newHead.Prev = null;

			head.Prev = newHead;
			head = newHead;
		}

		private void Reduce()
		{
			while (lookup.Count >= maxSize)
			{
				lookup.Remove(tail.Key);
				tail = tail.Prev;
				tail.Next = null;
			}
		}

		class Node
		{
			public Node Next { get; set; }
			public Node Prev { get; set; }
			public TKey Key { get; set; }
			public TValue Value { get; set; }
		}
	}
}
