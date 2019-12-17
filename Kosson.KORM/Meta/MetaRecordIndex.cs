using Kosson.KORM;
using System;
using System.Collections.Generic;

namespace Kosson.KORM.Meta
{
	class MetaRecordIndex : MetaObject, IMetaRecordIndex
	{
		private IMetaRecordField[] keyfields;
		private IMetaRecordField[] includedFields;

		public IMetaRecord Record { get; private set; }
		public string DBName { get; private set; }
		public bool IsUnique { get; private set; }
		public IReadOnlyCollection<IMetaRecordField> KeyFields { get { return keyfields; } }
		public IReadOnlyCollection<IMetaRecordField> IncludedFields { get { return includedFields; } }

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
			if (names == null) return new IMetaRecordField[0];
			var fields = new IMetaRecordField[names.Length];
			for (int i = 0; i < names.Length; i++)
			{
				var name = names[i];
				var field = Record.GetField(names[i]);
				if (field == null) throw new ArgumentException("Property " + name + " not found in type " + Record.Name + ".");
				fields[i] = field;
			}
			return fields;
		}
	}
}
