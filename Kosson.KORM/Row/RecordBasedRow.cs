using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Kosson.Interfaces
{
	/// <summary>
	/// Helper methods for creating IRow based on IRecord.
	/// </summary>
	public static class RecordBasedRow
	{
		private static Dictionary<Type, Func<IRecord, IRow>> builders = new Dictionary<Type, Func<IRecord, IRow>>();

		/// <summary>
		/// Creates a new IRow for a given record;
		/// </summary>
		/// <typeparam name="TRecord">Type of record to create a IRow from.</typeparam>
		/// <param name="record">Record to create a IRow from</param>
		/// <returns>IRow based on a given record</returns>
		public static IRow Create<TRecord>(TRecord record)
			where TRecord : IRecord
		{
			return CreateImpl<TRecord>(record);
		}

		/// <summary>
		/// Creates a new IRow for a given record;
		/// </summary>
		/// <param name="record">Record to create a IRow from</param>
		/// <returns>IRow based on a given record</returns>
		public static IRow Create(IRecord record)
		{
			var type = record.GetType();
			Func<IRecord, IRow> builder;
			lock (builders)
			{
				if (builders.TryGetValue(type, out builder)) return builder(record);
			}
			var builderMethod = typeof(IRow).GetMethod("CreateImpl", BindingFlags.Static | BindingFlags.NonPublic);
			builder = (Func<IRecord, IRow>)builderMethod.MakeGenericMethod(type).CreateDelegate(typeof(Func<IRecord, IRow>));
			lock (builders)
			{
				builders[type] = builder;
			}
			return builder(record);
		}


		private static IRow CreateImpl<TRecord>(IRecord record)
			where TRecord : IRecord
		{
			return new RecordBasedRow<TRecord>((TRecord)record);
		}
	}

	class RecordBasedRow<TRecord> : IRow
		where TRecord : IRecord
	{
		private static Func<TRecord, object>[] getters;
		private static string[] names;
		private static Dictionary<string, int> indices;
		private TRecord record;

		private static void PrepareInfo()
		{
			var meta = typeof(TRecord).Meta();
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

		private static object GetValue<TValue>(Func<TRecord, TValue> propertyGetter, TRecord record)
		{
			return propertyGetter(record);
		}

		static RecordBasedRow()
		{
			PrepareInfo();
		}

		public RecordBasedRow(TRecord record)
		{
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
			if (indices.TryGetValue(name, out index)) return index;
			throw new ArgumentOutOfRangeException("name", name, "Field not found.");
		}

		string IRow.GetName(int index)
		{
			if (index < 0 || index >= names.Length) throw new ArgumentOutOfRangeException("index", index, "Index out of range.");
			return names[index];
		}

		object IIndexBasedRow.this[int index]
		{
			get
			{
				if (index < 0 || index >= getters.Length) throw new ArgumentOutOfRangeException("index", index, "Index out of range.");
				return getters[index](record);
			}
		}

		int IIndexBasedRow.Length
		{
			get { return getters.Length; }
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return record.ToString();
		}
	}
}
