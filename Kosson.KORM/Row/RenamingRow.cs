using Kosson.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kosson.KRUD
{
	class RenamingRow : IRow
	{
		private IRow row;
		private Func<string, string> rename;

		public RenamingRow(IRow row, Func<string, string> rename)
		{
			this.row = row;
			this.rename = rename;
		}

		public object this[int index] { get { return row[index]; } }
		public object this[string name] { get { return row[GetIndex(name)]; } }
		public int Length { get { return row.Length; } }

		public int GetIndex(string name)
		{
			return row.GetIndex(rename(name));
		}

		public string GetName(int index)
		{
			return row.GetName(index);
		}
	}
}
