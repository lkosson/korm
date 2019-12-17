using System;
using System.IO;
using System.Xml;

namespace Kosson.KORM.Backup
{
	class XMLBackupWriter : IBackupWriter
	{
		private readonly IMetaBuilder metaBuilder;
		private readonly IPropertyBinder propertyBinder;
		private readonly IConverter converter;
		private readonly XmlWriter xmlwriter;

		public XMLBackupWriter(IMetaBuilder metaBuilder, IPropertyBinder propertyBinder, IConverter converter, Stream stream)
		{
			this.metaBuilder = metaBuilder;
			this.propertyBinder = propertyBinder;
			this.converter = converter;
			var xws = new XmlWriterSettings
			{
				Indent = true,
				IndentChars = "  ",
				OmitXmlDeclaration = true,
				NewLineOnAttributes = false,
				CloseOutput = false
			};
			xmlwriter = XmlWriter.Create(stream, xws);
			xmlwriter.WriteStartDocument();
			xmlwriter.WriteStartElement("records");
		}

		void IBackupWriter.WriteRecord(IRecord record)
		{
			xmlwriter.WriteStartElement("record");
			xmlwriter.WriteAttributeString("type", record.GetType().FullName);
			WriteFields(record);
			xmlwriter.WriteEndElement();
		}

		private void WriteFields(object target)
		{
			var meta = metaBuilder.Get(target.GetType());
			foreach (var field in meta.Fields)
			{
				if (!field.IsColumn) continue;
				if (field.IsReadOnly && !field.IsPrimaryKey) continue;
				var value = propertyBinder.Get(target, field.Name);
				if (value == null) continue;
				xmlwriter.WriteStartElement(field.Name);
				if (field.IsInline)
				{
					WriteFields(value);
				}
				else
				{
					if (value is IHasID) value = ((IHasID)value).ID;
					if (value is byte[]) value = Convert.ToBase64String((byte[])value);
					xmlwriter.WriteString(converter.Convert<string>(value));
				}
				xmlwriter.WriteEndElement(); // field
			}
		}

		void IDisposable.Dispose()
		{
			xmlwriter.WriteEndElement(); // record
			xmlwriter.WriteEndDocument();
			xmlwriter.Dispose();
		}
	}
}
