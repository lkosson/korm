using Kosson.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kosson.KRUD
{
	/// <summary>
	/// Read-only IRow implementation based on array of objects and dictionary of column names.
	/// </summary>
	class ArrayBasedRow : IRow
	{
		private object[] values;
		private Dictionary<string, int> names;

		int IIndexBasedRow.Length { get { return values.Length; } }
		object IIndexBasedRow.this[int index] { get { return index >= 0 && index < values.Length ? values[index] : null; } }
		object IRow.this[string name] { get { var row = (IRow)this; return row[row.GetIndex(name)]; } }

		/// <summary>
		/// Creates new row from provided values and column names.
		/// </summary>
		/// <param name="items">Array of values of the row.</param>
		/// <param name="names">Mapping from column names to column indices.</param>
		public ArrayBasedRow(object[] items, Dictionary<string, int> names)
		{
			this.values = items;
			this.names = names;
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

		/// <summary>
		/// Converts row to its string representation containing all column names and values.
		/// </summary>
		/// <returns>String representation of the row.</returns>
		public override string ToString()
		{
			return this.ToStringByColumns();
		}
	}
}
