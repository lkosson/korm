using Kosson.Interfaces;
using System;
using System.Linq;

namespace Kosson.KORM
{
	class MetaMappingRow : IIndexBasedRow
	{
		private int[] mapping;

		public IRow Row { get; set; }
		public object this[int index] { get { return index < 0 || index >= mapping.Length ? null : Row[mapping[index]]; } }
		public int Length { get { return mapping.Length; } }

		public MetaMappingRow(IRow template, IMetaRecordField[][] meta)
		{
			mapping = new int[meta.Length];
			for (int i = 0; i < meta.Length; i++)
			{
				var fieldspath = meta[i];
				var name = fieldspath.Length == 1 ? fieldspath[0].Name : String.Join(".", fieldspath.Select(f => f.Name));
				var index = template.GetIndex(name);
				mapping[i] = index;
			}
		}
	}
}
