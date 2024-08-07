﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace Kosson.KORM.Meta
{
	class MetaRecord : MetaObject, IMetaRecord
	{
		public const string PKNAME = "ID";

		private readonly List<MetaRecordField> fields;
		private readonly List<MetaRecordIndex> indices;
		private readonly Dictionary<string, MetaRecordField> fieldsLookup;

		public Type Type { get; private set; }
		public Type? TableType { get; private set; }
		public bool IsTable { get; private set; }
		public string Name { get; private set; }
		public string? DBName { get; private set; }
		public string? DBSchema { get; private set; }
		public string? DBPrefix { get; private set; }
		public string? DBQuery { get; private set; }
		public bool IsManualID { get; private set; }
		public bool IsConverted { get; private set; }
		public IMetaRecordField? InliningField { get; private set; }
		public IReadOnlyCollection<IMetaRecordField> Fields => fields;
		public IReadOnlyCollection<IMetaRecordIndex> Indices => indices;
		public IMetaRecordField? RowVersion => GetField(IRecordWithRowVersionINT.NAME);
		public IMetaRecordField PrimaryKey => GetField(PKNAME) ?? throw new ArgumentException("type", "Type " + Name + " does not have primary key.");

		public MetaRecord(IFactory factory, Type record, IMetaRecordField? inlineParent = null)
		{
			fields = [];
			indices = [];
			fieldsLookup = [];
			Type = record;
			Name = GenerateName(record);

			if (inlineParent == null)
			{
				DBName = Name;
			}
			else
			{
				DBName = inlineParent.Record.DBName;
				DBSchema = inlineParent.Record.DBSchema;
				DBPrefix = inlineParent.InlinePrefix;
				InliningField = inlineParent;
			}
			ProcessType(record, factory);
		}

		IMetaRecordField? IMetaRecord.GetField(string name)
		{
			var field = GetField(name);
			if (field != null) return field;
			var dot = name.IndexOf('.');
			if (dot <= 0) return null;
			field = GetField(name.Substring(0, dot));
			if (field == null) return null;
			var tail = name.Substring(dot + 1);
			if (field.ForeignMeta != null) return field.ForeignMeta.GetField(tail);
			if (field.InlineRecord != null) return field.InlineRecord.GetField(tail);
			return null;
		}

		private MetaRecordField? GetField(string name)
		{
			if (fieldsLookup.TryGetValue(name, out var field)) return field;
			return null;
		}

		string IMetaRecord.GetFieldTableAlias(string name)
		{
			var path = new List<string>();
			path.Add(Name);
			GetFieldTableAlias(this, name, path);
			return String.Join(".", path);
		}

		private static void GetFieldTableAlias(IMetaRecord meta, string name, List<string> path)
		{
			IMetaRecordField? field = ((MetaRecord)meta).GetField(name);
			if (field != null) return;
			var dot = name.IndexOf('.');
			if (dot <= 0) return;
			field = meta.GetField(name.Substring(0, dot));
			if (field == null) return;
			var tail = name.Substring(dot + 1);
			if (field.ForeignMeta != null)
			{
				path.Add(field.Name);
				GetFieldTableAlias(field.ForeignMeta, tail, path);
			}
			if (field.InlineRecord != null) GetFieldTableAlias(field.InlineRecord, tail, path);
		}

		private void ProcessType(Type record, IFactory factory)
		{
			ProcessRecursive(record, ProcessTableAttribute);
			ProcessRecursive(record, ProcessDBNameAttribute);
			ProcessRecursive(record, ProcessDBSchemaAttribute);

			// Properties processing requires complete (incl. derived types) record metadata for prefix generation
			ProcessRecursive(record, type => ProcessProperties(type, factory));

			// Indices processing requires properties
			ProcessRecursive(record, ProcessIndexAttributes);
		}

		private static void ProcessRecursive(Type record, Action<Type> processor)
		{
			var typeInfo = record.GetTypeInfo();
			if (typeInfo.BaseType != null) ProcessRecursive(typeInfo.BaseType, processor);
			processor(record);
		}

		private void ProcessDBNameAttribute(Type record)
		{
			var dbname = (DBNameAttribute?)record.GetTypeInfo().GetCustomAttribute(typeof(DBNameAttribute), false);
			if (dbname == null) return;
			DBName = dbname.Name;
		}

		private void ProcessDBSchemaAttribute(Type record)
		{
			var dbschema = (DBSchemaAttribute?)record.GetTypeInfo().GetCustomAttribute(typeof(DBSchemaAttribute), false);
			if (dbschema == null) return;
			DBSchema = dbschema.Name;
		}

		private void ProcessTableAttribute(Type record)
		{
			var table = (TableAttribute?)record.GetTypeInfo().GetCustomAttribute(typeof(TableAttribute), false);
			if (table == null) return;

			if (TableType == null) TableType = record;
			IsTable = true;
			DBName = GenerateName(record);
			DBPrefix = table.Prefix ?? GeneratePrefix(DBName);
			IsManualID = table.IsManualID;
			DBQuery = table.Query;
			IsConverted |= table.IsConverted;
		}

		private static string GenerateName(Type type)
		{
			if (type.DeclaringType == null) return type.Name;
			else return type.DeclaringType.Name + type.Name;
		}

		private static string GeneratePrefix(string name)
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
			if (prefix.Length < 2) prefix = name.Substring(0, Math.Min(name.Length, 3));
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

		private void ProcessProperties(Type record, IFactory factory)
		{
			// Process DeclaredOnly to prefer properties in derived classes over properties from base classes when
			// property is hidden in derived class.
			var properties = record.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
			foreach (var property in properties)
			{
				ProcessProperty(property, factory);
			}
		}

		private void ProcessProperty(PropertyInfo property, IFactory factory)
		{
			var field = new MetaRecordField(property, this, factory);
			var existing = GetField(field.Name);
			if (existing == null)
			{
				fieldsLookup[field.Name] = field;
				fields.Add(field);
			}
			else
			{
				existing.Update(property, factory);
			}
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
