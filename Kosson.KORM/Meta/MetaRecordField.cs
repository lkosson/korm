using System;
using System.Data;
using System.Reflection;

namespace Kosson.KORM.Meta
{
	class MetaRecordField : MetaObject, IMetaRecordField
	{
		public IMetaRecord Record { get; private set; }
		public PropertyInfo Property { get; private set; }

		public string Name { get; private set; }
		public Type Type { get; private set; }
		public bool IsEagerLookup { get; private set; }
		public bool IsRecordRef { get; private set; }
		public bool IsPrimaryKey { get; private set; }

		public bool IsColumn { get; private set; }
		public bool IsFromDB { get; private set; }
		public string DBName { get; private set; }
		public DbType DBType { get; private set; }
		public string ColumnDefinition { get; private set; }
		public bool IsReadOnly { get; private set; }

		public int Length { get; private set; }
		public int Precision { get; private set; }
		public bool Trim { get; private set; }

		public bool IsForeignKey { get; private set; }
		public bool IsCascade { get; private set; }
		public bool IsSetNull { get; private set; }
		public Type ForeignType { get; private set; }

		public SubqueryBuilder SubqueryBuilder { get; private set; }

		public object DefaultValue { get; private set; }
		public bool IsNotNull { get; private set; }

		public bool IsInline { get; private set; }
		public string InlinePrefix { get; private set; }
		public IMetaRecord InlineRecord { get; private set; }

		public MetaRecordField(PropertyInfo property, IMetaRecord record, IFactory factory)
		{
			Record = record;
			Update(property, factory);
		}

		internal void Update(PropertyInfo property, IFactory factory)
		{
			ProcessProperty(property);
			ProcessDBNameAttribute(property);
			ProcessColumnAttribute(property, factory);
			ProcessForeignKeyAttribute(property);
			ProcessSubqueryAttribute(property);
			ProcessInlineAttribute(property, factory);
		}

		private void ProcessProperty(PropertyInfo property)
		{
			Property = property;
			Name = property.Name;
			IsPrimaryKey = Name == MetaRecord.PKNAME;

			// Property type can change in derived class if new property is declared with same name
			Type = property.PropertyType;
			IsEagerLookup = typeof(IRecord).IsAssignableFrom(Type);
			IsRecordRef = typeof(IRecordRef).IsAssignableFrom(Type);
		}

		private void ProcessDBNameAttribute(PropertyInfo property)
		{
			var dbname = (DBNameAttribute)property.GetCustomAttribute(typeof(DBNameAttribute), false);
			if (dbname == null)
				DBName = String.IsNullOrEmpty(Record.DBPrefix) ? Name : Record.DBPrefix + "_" + Name;
			else
				DBName = dbname.Name;
		}

		private void ProcessColumnAttribute(PropertyInfo property, IFactory factory)
		{
			var column = (ColumnAttribute)property.GetCustomAttribute(typeof(ColumnAttribute), false);
			if (column == null) return;

			IsColumn = true;
			IsFromDB = true;
			DBType = column.DBType == DbType.Object ? ResolveDBType(Type) : column.DBType;
			Length = column.Length;
			Precision = column.Precision;
			Trim = column.Trim;
			ColumnDefinition = column.ColumnDefinition;
			IsReadOnly = column.IsReadOnly || (IsPrimaryKey && !Record.IsManualID);
			SubqueryBuilder = null;

			IsNotNull = column.IsNotNull;
			if (column.HasDefaultValue)
			{
				var template = factory.Create(Record.Type);
				DefaultValue = property.GetValue(template);
			}
		}

		private DbType ResolveDBType(Type type)
		{
			var nullable = Nullable.GetUnderlyingType(type);
			if (nullable != null) type = nullable;
			if (type == typeof(bool)) return DbType.Boolean;
			if (type == typeof(byte)) return DbType.Byte;
			if (type == typeof(short)) return DbType.Int16;
			if (type == typeof(int)) return DbType.Int32;
			if (type == typeof(long)) return DbType.Int64;
			if (type == typeof(float)) return DbType.Single;
			if (type == typeof(double)) return DbType.Double;
			if (type == typeof(decimal)) return DbType.Decimal;
			if (type == typeof(string)) return DbType.String;
			if (type == typeof(byte[])) return DbType.Binary;
			if (type == typeof(Guid)) return DbType.Guid;
			if (type == typeof(DateTime)) return DbType.DateTime2;
			if (IsRecordRef || IsEagerLookup) return DbType.Int64;
			if (type.GetTypeInfo().IsEnum) return DbType.Int32;
			return DbType.Object;
		}

		private void ProcessForeignKeyAttribute(PropertyInfo property)
		{
			var fk = (ForeignKeyAttribute)property.GetCustomAttribute(typeof(ForeignKeyAttribute), false);
			if (fk == null) return;

			IsForeignKey = true;
			IsCascade = fk.IsCascade;
			IsSetNull = fk.IsSetNull;

			if (IsRecordRef)
			{
				ForeignType = Type.GetGenericArguments()[0];
			}
			else
			{
				ForeignType = Type;
			}
		}

		private void ProcessSubqueryAttribute(PropertyInfo property)
		{
			var subquery = (SubqueryBuilderAttribute)property.GetCustomAttribute(typeof(SubqueryBuilderAttribute), false);
			if (subquery == null) return;

			IsForeignKey = false;
			IsColumn = false;
			IsFromDB = true;
			SubqueryBuilder = subquery.Build;
		}

		private void ProcessInlineAttribute(PropertyInfo property, IFactory factory)
		{
			var inline = (InlineAttribute)property.GetCustomAttribute(typeof(InlineAttribute), false);
			if (inline == null) return;

			IsColumn = true;
			IsFromDB = true;
			IsInline = true;
			if (inline.Prefix == null)
				InlinePrefix = DBName;
			else if (String.IsNullOrEmpty(Record.DBPrefix))
				InlinePrefix = inline.Prefix;
			else
				InlinePrefix = Record.DBPrefix + "_" + inline.Prefix;
			InlineRecord = new MetaRecord(factory, property.PropertyType, this);
		}

		public override string ToString()
		{
			return Record + "." + Name;
		}
	}
}
