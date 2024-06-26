using System;
using System.Collections.Generic;

namespace Kosson.KORM.Meta
{
	class MetaRecordIndex : MetaObject, IMetaRecordIndex
	{
		private readonly IMetaRecordField[] keyfields;
		private readonly IMetaRecordField[] includedFields;

		public IMetaRecord Record { get; private set; }
		public string DBName { get; private set; }
		public bool IsUnique { get; private set; }
		public IReadOnlyCollection<IMetaRecordField> KeyFields => keyfields;
		public IReadOnlyCollection<IMetaRecordField> IncludedFields => includedFields;

		public MetaRecordIndex(IndexAttribute index, IMetaRecord record)
		{
			Record = record;
			DBName = index.Name;
			IsUnique = index.IsUnique;
			keyfields = ResolveFields(index.Fields);
			includedFields = ResolveFields(index.IncludedFields);
		}

		private IMetaRecordField[] ResolveFields(string[] names)
		{
			if (names == null) return [];
			var fields = new IMetaRecordField[names.Length];
			for (int i = 0; i < names.Length; i++)
			{
				var name = names[i];
				var field = Record.GetField(name);
				fields[i] = field ?? throw new ArgumentException("Property " + name + " not found in type " + Record.Name + ".");
			}
			return fields;
		}
	}
}
