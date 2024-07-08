using System.Collections.Generic;

namespace Kosson.KORM
{
	/// <summary>
	/// Mutable IRow implementation based on list of values
	/// </summary>
	public class HashRow : IRow
	{
		private readonly List<object> values;
		private readonly Dictionary<string, int> names;

		int IIndexBasedRow.Length => values.Count;
		object? IIndexBasedRow.this[int index] => index >= 0 && index < values.Count ? values[index] : null;
		object? IRow.this[string name] { get { var row = (IRow)this; return row[row.GetIndex(name)]; } }

		/// <summary>
		/// Creates new, empty row.
		/// </summary>
		public HashRow()
		{
			names = [];
			values = [];
		}

		/// <summary>
		/// Adds new key-value pair to the row.
		/// </summary>
		/// <param name="name">Name of a column.</param>
		/// <param name="value">Value to add.</param>
		public void Add(string name, object value)
		{
			if (names.TryGetValue(name, out var index))
			{
				values[index] = value;
			}
			else
			{
				names[name] = values.Count;
				values.Add(value);
			}
		}

		int IRow.GetIndex(string name)
		{
			if (names.TryGetValue(name, out var index)) return index;
			return -1;
		}

		string? IRow.GetName(int index)
		{
			foreach (var pair in names)
			{
				if (pair.Value == index) return pair.Key;
			}
			return null;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.ToStringByColumns();
		}
	}
}
