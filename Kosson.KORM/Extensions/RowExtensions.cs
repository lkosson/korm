#define noLAZY
using Kosson.KORM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kosson.KORM
{
	/// <summary>
	/// Extension methods for Kosson.Interfaces.IRow
	/// </summary>
	public static class IRowExtensions
	{
		/// <summary>
		/// Creates records based on a given rows with properties filled with values from corresponding row columns.
		/// </summary>
		/// <typeparam name="T">Type of record to create.</typeparam>
		/// <param name="rows">Rows to use as a source of properties values for records.</param>
		/// <returns>Records built from given rows.</returns>
#if LAZY
		public static IEnumerable<T> Load<T>(this IEnumerable<IRow> rows) where T : class, new()
#else
		public static IReadOnlyCollection<T> Load<T>(this IEnumerable<IRow> rows, IConverter converter, IRecordLoader recordLoader, IFactory factory) where T : class, new()
#endif
		{
			var template = rows.FirstOrDefault();
#if LAZY
			if (template == null) yield break;
#else
			if (template == null) return new T[0];

			List<T> records;
			if (rows is ICollection)
				records = new List<T>(((ICollection)rows).Count);
			else
				records = new List<T>();
#endif
			var helper = new LoaderHelper<T>(template, converter, recordLoader, factory);
			foreach (var row in rows)
			{
				var record = helper.constructor();
				var notify = record as IRecordNotifySelect;
				if (notify != null)
				{
					var result = notify.OnSelect(row);
					if (result == RecordNotifyResult.Skip) continue;
					if (result == RecordNotifyResult.Break) break;
				}

				helper.Load(row, record);

				if (notify == null)
				{
#if LAZY
					yield return record;
#else
					records.Add(record);
#endif
				}
				else
				{
					var result = notify.OnSelected(row);
					if (result == RecordNotifyResult.Skip) continue;
#if LAZY
					yield return record;
#else
					records.Add(record);
#endif
					if (result == RecordNotifyResult.Break) break;
				}
			}
#if !LAZY
			return records;
#endif
		}

		/// <summary>
		/// Returns string representation of given row, listing all columns and their values.
		/// </summary>
		/// <param name="row">Row to convert to string.</param>
		/// <returns>String represtatntion of the row.</returns>
		public static string ToStringByColumns(this IRow row)
		{
			var sb = new StringBuilder();
			sb.Append("Row {");
			bool first = true;
			for (int i = 0; i < row.Length; i++)
			{
				if (first)
				{
					first = false;
				}
				else
				{
					sb.Append(", ");
				}
				sb.Append(row.GetName(i) + "=" + row[i]);
			}
			sb.Append(" }");
			return sb.ToString();
		}

		class LoaderHelper<T> where T : class, new()
		{
			private IConverter converter;
			private MetaMappingRow mappingRow;
			public Func<T> constructor;
			private LoaderByIndexDelegate<T> loader;

			public LoaderHelper(IRow template, IConverter converter, IRecordLoader recordLoader, IFactory factory)
			{
				this.converter = converter;
				constructor = (Func<T>)factory.GetConstructor(typeof(T));
				loader = recordLoader.GetLoader<T>(out var fieldMapping);
				mappingRow = new MetaMappingRow(template, fieldMapping);
			}

			public void Load(IRow row, T target)
			{
				mappingRow.Row = row;
				loader(target, mappingRow, converter, null);
			}
		}
	}
}
