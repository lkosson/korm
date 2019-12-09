using Kosson.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Kosson.KRUD.Meta
{
	class MetaRecord : MetaObject, IMetaRecord
	{
		public const string PKNAME = "ID";

		private List<MetaRecordField> fields;
		private List<MetaRecordIndex> indices;
		private Dictionary<string, MetaRecordField> fieldsLookup;

		public Type Type { get; private set; }
		public Type TableType { get; private set; }
		public bool IsTable { get; private set; }
		public string Name { get; private set; }
		public string DBName { get; private set; }
		public string DBPrefix { get; private set; }
		public string DBQuery { get; private set; }
		public bool IsManualID { get; private set; }
		public IMetaRecordField InliningField { get; private set; }
		public IReadOnlyCollection<IMetaRecordField> Fields { get { return fields; } }
		public IReadOnlyCollection<IMetaRecordIndex> Indices { get { return indices; } }
		public IMetaRecordField RowVersion { get { return GetField(IRecordWithRowVersionINT.NAME); } }
		public IMetaRecordField PrimaryKey
		{
			get
			{
				var idfield = GetField(PKNAME);
				if (idfield == null) throw new ArgumentException("type", "Type " + Name + " does not have primary key.");
				return idfield;
			}
		}

		public MetaRecord(Type record) : this(record, null)
		{
		}

		public MetaRecord(Type record, IMetaRecordField inlineParent)
		{
			fields = new List<MetaRecordField>();
			indices = new List<MetaRecordIndex>();
			fieldsLookup = new Dictionary<string, MetaRecordField>();
			Type = record;
			Name = GenerateName(record);

			if (inlineParent == null)
			{
				DBName = Name;
			}
			else
			{
				DBName = inlineParent.Record.DBName;
				DBPrefix = inlineParent.InlinePrefix;
				InliningField = inlineParent;
			}
			ProcessType(record);
		}

		IMetaRecordField IMetaRecord.GetField(string name)
		{
			IMetaRecordField field = GetField(name);
			if (field == null)
			{
				var dot = name.IndexOf('.');
				if (dot > 0)
				{
					field = GetField(name.Substring(0, dot));
					if (field != null)
					{
						if (field.InlineRecord == null) return null;
						field = field.InlineRecord.GetField(name.Substring(dot + 1));
					}
				}
			}
			return field;
		}

		private MetaRecordField GetField(string name)
		{
			MetaRecordField field;
			if (fieldsLookup.TryGetValue(name, out field)) return field;
			return null;
		}

		private void ProcessType(Type record)
		{
			ProcessRecursive(record, ProcessTableAttribute);
			ProcessRecursive(record, ProcessDBNameAttribute);

			// Properties processing requires complete (incl. derived types) record metadata for prefix generation
			ProcessRecursive(record, ProcessProperties);

			// Indices processing requires properties
			ProcessRecursive(record, ProcessIndexAttributes);
		}

		private void ProcessRecursive(Type record, Action<Type> processor)
		{
			var typeInfo = record.GetTypeInfo();
			if (typeInfo.BaseType != null) ProcessRecursive(typeInfo.BaseType, processor);
			processor(record);
		}

		private void ProcessDBNameAttribute(Type record)
		{
			var dbname = (DBNameAttribute)record.GetTypeInfo().GetCustomAttribute(typeof(DBNameAttribute), false);
			if (dbname == null) return;
			DBName = dbname.Name;
		}

		private void ProcessTableAttribute(Type record)
		{
			var table = (TableAttribute)record.GetTypeInfo().GetCustomAttribute(typeof(TableAttribute), false);
			if (table == null) return;

			if (TableType == null) TableType = record;
			IsTable = true;
			DBName = GenerateName(record);
			DBPrefix = table.Prefix ?? GeneratePrefix(DBName);
			IsManualID = table.IsManualID;
			DBQuery = table.Query;
		}

		private string GenerateName(Type type)
		{
			if (type.DeclaringType == null)
				return type.Name;
			else
				return type.DeclaringType.Name + type.Name;
		}

		private string GeneratePrefix(string name)
		{
			string prefix = "";
			bool prevUpper = false;
			for (int i = 0; i < name.Length; i++)
			{
				bool upper = Char.IsUpper(name[i]);
				// Skip consecutive caps - eg. "TableVAT" gets "tv" prefix instead of "tvat".
				if (upper && !prevUpper) prefix += name[i].ToString();
				prevUpper = upper;
			}
			if (prefix.Length < 2) prefix = name.Substring(0, 3);
			prefix = prefix.ToLower();
			return prefix;
		}

		private void ProcessIndexAttributes(Type record)
		{
			var attributes = (IndexAttribute[])record.GetTypeInfo().GetCustomAttributes(typeof(IndexAttribute), false);
			foreach (var attribute in attributes)
			{
				ProcessIndexAttribute(attribute);
			}
		}

		private void ProcessIndexAttribute(IndexAttribute attribute)
		{
			var index = new MetaRecordIndex(attribute, this);
			for (int i = 0; i < indices.Count; i++)
			{
				if (indices[i].DBName == index.DBName) indices[i] = index;
			}
			indices.Add(index);
		}

		private void ProcessProperties(Type record)
		{
			// Process DeclaredOnly to prefer properties in derived classes over properties from base classes when
			// property is hidden in derived class.
			var properties = record.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
			foreach (var property in properties)
			{
				ProcessProperty(property);
			}
		}

		private void ProcessProperty(PropertyInfo property)
		{
			var field = new MetaRecordField(property, this);
			var existing = GetField(field.Name);
			if (existing == null)
			{
				fieldsLookup[field.Name] = field;
				fields.Add(field);
			}
			else
			{
				existing.Update(property, this);
			}
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
