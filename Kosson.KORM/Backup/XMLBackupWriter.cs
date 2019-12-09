using Kosson.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Kosson.KRUD
{
	/// <summary>
	/// Low-performance, XML-based record serializer.
	/// </summary>
	public class XMLBackupWriter : IBackupWriter
	{
		private IMetaBuilder metaBuilder;
		private IPropertyBinder propertyBinder;
		private IConverter converter;
		private XmlWriter xw;

		/// <summary>
		/// Creates a new backup serializer writing XML to provided stream.
		/// </summary>
		/// <param name="stream">Steram to write XML to.</param>
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
			xw = XmlWriter.Create(stream, xws);
			xw.WriteStartDocument();
			xw.WriteStartElement("records");
		}

		/// <summary>
		/// Creates a new XML file containing all records of provided types.
		/// </summary>
		/// <param name="file">Name of XML file to create.</param>
		/// <param name="tables">Types of records to include in backup.</param>
		public static void Run(IMetaBuilder metaBuilder, IPropertyBinder propertyBinder, IConverter converter, string file, IEnumerable<Type> tables)
		{
			using (var fs = new FileStream(file, FileMode.Create))
			using (var bw = new XMLBackupWriter(metaBuilder, propertyBinder, converter, fs))
			{
				var bs = KORMContext.Current.BackupProvider.CreateBackupSet(bw);
				foreach (var table in tables) bs.AddTable(table);
			}
		}

		/// <inheritdoc/>
		public void WriteRecord(IRecord record)
		{
			xw.WriteStartElement("record");
			xw.WriteAttributeString("type", record.GetType().FullName);
			WriteFields(record);
			xw.WriteEndElement();
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
				xw.WriteStartElement(field.Name);
				if (field.IsInline)
				{
					WriteFields(value);
				}
				else
				{
					if (value is IHasID) value = ((IHasID)value).ID;
					if (value is byte[]) value = Convert.ToBase64String((byte[])value);
					xw.WriteString(converter.Convert<string>(value));
				}
				xw.WriteEndElement(); // field
			}
		}

		void IDisposable.Dispose()
		{
			xw.WriteEndElement(); // record
			xw.WriteEndDocument();
			xw.Dispose();
		}
	}
}
