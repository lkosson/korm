using System.Collections.Generic;

namespace Kosson.Interfaces
{
	/// <summary>
	/// Mutable IRow implementation based on list of values
	/// </summary>
	public class HashRow : IRow
	{
		private List<object> values;
		private Dictionary<string, int> names;

		int IIndexBasedRow.Length { get { return values.Count; } }
		object IIndexBasedRow.this[int index] { get { return index >= 0 && index < values.Count ? values[index] : null; } }
		object IRow.this[string name] { get { var row = (IRow)this; return row[row.GetIndex(name)]; } }

		/// <summary>
		/// Creates new, empty row.
		/// </summary>
		public HashRow()
		{
			names = new Dictionary<string, int>();
			values = new List<object>();
		}

		/// <summary>
		/// Adds new key-value pair to the row.
		/// </summary>
		/// <param name="name">Name of a column.</param>
		/// <param name="value">Value to add.</param>
		public void Add(string name, object value)
		{
			int index;
			if (names.TryGetValue(name, out index))
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
			int index;
			if (names.TryGetValue(name, out index)) return index;
			return -1;
		}

		string IRow.GetName(int index)
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
