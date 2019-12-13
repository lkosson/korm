using Kosson.Interfaces;
using Kosson.KORM.DB;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace Kosson.KORM.Backup
{
	class XMLBackupReader : IBackupReader
	{
		private readonly IMetaBuilder metaBuilder;
		private readonly IConverter converter;
		private readonly IFactory factory;
		private readonly XmlReader xmlreader;
		private readonly Dictionary<string, Type> typeMapping;

		public XMLBackupReader(IMetaBuilder metaBuilder, IConverter converter, IFactory factory, Stream stream)
		{
			typeMapping = new Dictionary<string, Type>();
			this.metaBuilder = metaBuilder;
			this.converter = converter;
			this.factory = factory;
			var xrs = new XmlReaderSettings
			{
				IgnoreComments = true,
				IgnoreWhitespace = true
			};
			xmlreader = XmlReader.Create(stream, xrs);
			xmlreader.ReadStartElement("records");
		}

		IRecord IBackupReader.ReadRecord()
		{
			if (xmlreader.NodeType == XmlNodeType.EndElement && xmlreader.Name == "records")
			{
				xmlreader.ReadEndElement();
				return null;
			}

			while (!xmlreader.IsStartElement("record") && xmlreader.Read()) ;

			string typeName = null;
			while (xmlreader.MoveToNextAttribute())
			{
				if (xmlreader.Name != "type") continue;
				typeName = xmlreader.Value;
			}
			xmlreader.MoveToElement();

			if (typeName == null) throw new KORMBackupException(XMLPositionInfo() + "Missing \"type\" attribute.");

			var type = ResolveType(typeName);
			var record = (IRecord)factory.Create(type);
			xmlreader.ReadStartElement("record");
			ReadFields(record);
			xmlreader.ReadEndElement();
			return record;
		}

		private void ReadFields(object target)
		{
			var meta = metaBuilder.Get(target.GetType());
			while (xmlreader.NodeType == XmlNodeType.Element)
			{
				if (xmlreader.IsEmptyElement)
				{
					xmlreader.Read();
					continue;
				}

				var name = xmlreader.Name;
				var field = meta.GetField(name);
				if (field == null) throw new KORMBackupException(XMLPositionInfo() + "Invalid field name: " + name);

				xmlreader.Read();
				if (xmlreader.NodeType == XmlNodeType.Element)
				{
					var inline = factory.Create(field.Type);
					ReadFields(inline);
					field.Property.SetValue(target, inline);
					xmlreader.ReadEndElement();
				}
				else if (xmlreader.NodeType == XmlNodeType.Text)
				{
					object value;
					var text = xmlreader.Value;
					xmlreader.Read();
					if (field.IsRecordRef)
					{
						var recordref = (IRecordRef)factory.Create(field.Type);
						recordref.ID = Int64.Parse(text, CultureInfo.InvariantCulture);
						value = recordref;
					}
					else if (field.IsForeignKey)
					{
						var record = (IRecord)factory.Create(field.Type);
						record.ID = Int64.Parse(text, CultureInfo.InvariantCulture);
						value = record;
					}
					else
					{
						if (field.Type == typeof(byte[]))
							value = Convert.FromBase64String(text);
						else
							value = converter.Convert(text, field.Type);
					}
					field.Property.SetValue(target, value);
					xmlreader.ReadEndElement();
				}
				else if (xmlreader.NodeType == XmlNodeType.EndElement)
				{
					var value = converter.Convert("", field.Type);
					field.Property.SetValue(target, value);
					xmlreader.ReadEndElement();
				}
			}
		}

		private Type ResolveType(string typeName)
		{
			Type type;
			if (!typeMapping.TryGetValue(typeName, out type))
			{
				foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
				{
					type = asm.GetType(typeName);
					if (type != null)
					{
						typeMapping[typeName] = type;
						return type;
					}
				}
				throw new KORMBackupException(XMLPositionInfo() + "Unknown type: " + typeName);
			}
			return type;
		}

		private string XMLPositionInfo()
		{
			var li = xmlreader as IXmlLineInfo;
			if (li == null) return "";
			return "Line " + li.LineNumber + ", position " + li.LinePosition + ": ";
		}

		void IDisposable.Dispose()
		{
			xmlreader.Dispose();
		}
	}
}
