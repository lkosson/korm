using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Kosson.Interfaces
{
	/// <summary>
	/// Helper methods for creating IRow based on IRecord.
	/// </summary>
	public static class RecordBasedRow
	{
		private static Dictionary<Type, Func<IMetaRecord, IRecord, IRow>> builders = new Dictionary<Type, Func<IMetaRecord, IRecord, IRow>>();

		/// <summary>
		/// Creates a new IRow for a given record;
		/// </summary>
		/// <typeparam name="TRecord">Type of record to create a IRow from.</typeparam>
		/// <param name="record">Record to create a IRow from</param>
		/// <returns>IRow based on a given record</returns>
		public static IRow Create<TRecord>(IMetaRecord metaRecord, TRecord record)
			where TRecord : IRecord
		{
			return CreateImpl<TRecord>(metaRecord, record);
		}

		/// <summary>
		/// Creates a new IRow for a given record;
		/// </summary>
		/// <param name="record">Record to create a IRow from</param>
		/// <returns>IRow based on a given record</returns>
		public static IRow Create(IMetaRecord meta, IRecord record)
		{
			var type = record.GetType();
			Func<IMetaRecord, IRecord, IRow> builder;
			lock (builders)
			{
				if (builders.TryGetValue(type, out builder)) return builder(meta, record);
			}
			var builderMethod = typeof(RecordBasedRow).GetMethod(nameof(RecordBasedRow.CreateImpl), BindingFlags.Static | BindingFlags.NonPublic);
			builder = (Func<IMetaRecord, IRecord, IRow>)builderMethod.MakeGenericMethod(type).CreateDelegate(typeof(Func<IMetaRecord, IRecord, IRow>));
			lock (builders)
			{
				builders[type] = builder;
			}
			return builder(meta, record);
		}


		private static IRow CreateImpl<TRecord>(IMetaRecord metaRecord, IRecord record)
			where TRecord : IRecord
		{
			return new RecordBasedRow<TRecord>(metaRecord, (TRecord)record);
		}
	}

	class RecordBasedRow<TRecord> : IRow
		where TRecord : IRecord
	{
		private static RecordBasedRowInfo info;
		private TRecord record;

		private static object GetValue<TValue>(Func<TRecord, TValue> propertyGetter, TRecord record)
		{
			return propertyGetter(record);
		}

		public RecordBasedRow(IMetaRecord meta, TRecord record)
		{
			if (info == null) info = new RecordBasedRowInfo(meta);
			this.record = record;
		}

		object IRow.this[string name]
		{
			get 
			{
				var row = (IRow)this;
				return row[row.GetIndex(name)]; 
			}
		}

		int IRow.GetIndex(string name)
		{
			int index;
			if (info.indices.TryGetValue(name, out index)) return index;
			throw new ArgumentOutOfRangeException("name", name, "Field not found.");
		}

		string IRow.GetName(int index)
		{
			if (index < 0 || index >= info.names.Length) throw new ArgumentOutOfRangeException("index", index, "Index out of range.");
			return info.names[index];
		}

		object IIndexBasedRow.this[int index]
		{
			get
			{
				if (index < 0 || index >= info.getters.Length) throw new ArgumentOutOfRangeException("index", index, "Index out of range.");
				return info.getters[index](record);
			}
		}

		int IIndexBasedRow.Length
		{
			get { return info.getters.Length; }
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return record.ToString();
		}

		class RecordBasedRowInfo
		{
			public Func<TRecord, object>[] getters;
			public string[] names;
			public Dictionary<string, int> indices;

			public RecordBasedRowInfo(IMetaRecord meta)
			{
				var fields = meta.Fields;
				getters = new Func<TRecord, object>[fields.Count];
				indices = new Dictionary<string, int>();
				names = new string[fields.Count];
				int i = 0;
				var getValueMethod = typeof(RecordBasedRow<TRecord>).GetMethod("GetValue", BindingFlags.Static | BindingFlags.NonPublic);
				foreach (var field in fields)
				{
					var getMethod = field.Property.GetMethod;
					var propertyDelegateType = Expression.GetDelegateType(typeof(TRecord), getMethod.ReturnType);
					var propertyGetter = getMethod.CreateDelegate(propertyDelegateType);
					var getValueTypedMethod = getValueMethod.MakeGenericMethod(getMethod.ReturnType);
					var getValue = (Func<TRecord, object>)getValueTypedMethod.CreateDelegate(typeof(Func<TRecord, object>), propertyGetter);
					indices[field.Name] = i;
					getters[i] = getValue;
					names[i] = field.Name;
					i++;
				}
			}
		}
	}
}
