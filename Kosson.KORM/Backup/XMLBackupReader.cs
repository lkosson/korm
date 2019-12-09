using Kosson.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Kosson.KRUD
{
	/// <summary>
	/// Low-performance, XML-based record deserializer.
	/// </summary>
	public class XMLBackupReader : IBackupReader
	{
		private XmlReader xr;
		private Dictionary<string, Type> typeMapping;
		private IMetaBuilder metaBuilder;
		private IConverter converter;
		private IFactory factory;

		/// <summary>
		/// Creates a new record deserializer reading from a given stream.
		/// </summary>
		/// <param name="stream">Stream to read data from.</param>
		public XMLBackupReader(Stream stream)
		{
			typeMapping = new Dictionary<string, Type>();
			metaBuilder = KORMContext.Current.MetaBuilder;
			converter = KORMContext.Current.Converter;
			factory = KORMContext.Current.Factory;
			var xrs = new XmlReaderSettings
			{
				IgnoreComments = true,
				IgnoreWhitespace = true
			};
			xr = XmlReader.Create(stream, xrs);
			xr.ReadStartElement("records");
		}

		/// <inheritdoc />
		public IRecord ReadRecord()
		{
			if (xr.NodeType == XmlNodeType.EndElement && xr.Name == "records")
			{
				xr.ReadEndElement();
				return null;
			}

			while (!xr.IsStartElement("record") && xr.Read()) ;

			string typeName = null;
			while (xr.MoveToNextAttribute())
			{
				if (xr.Name != "type") continue;
				typeName = xr.Value;
			}
			xr.MoveToElement();

			if (typeName == null) throw new KRUDBackupException(XMLPositionInfo() + "Missing \"type\" attribute.");

			var type = ResolveType(typeName);
			var record = (IRecord)factory.Create(type);
			xr.ReadStartElement("record");
			ReadFields(record);
			xr.ReadEndElement();
			return record;
		}

		private void ReadFields(object target)
		{
			var meta = metaBuilder.Get(target.GetType());
			while (xr.NodeType == XmlNodeType.Element)
			{
				if (xr.IsEmptyElement)
				{
					xr.Read();
					continue;
				}

				var name = xr.Name;
				var field = meta.GetField(name);
				if (field == null) throw new KRUDBackupException(XMLPositionInfo() + "Invalid field name: " + name);

				xr.Read();
				if (xr.NodeType == XmlNodeType.Element)
				{
					var inline = factory.Create(field.Type);
					ReadFields(inline);
					field.Property.SetValue(target, inline);
					xr.ReadEndElement();
				}
				else if (xr.NodeType == XmlNodeType.Text)
				{
					object value;
					var text = xr.Value;
					xr.Read();
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
					xr.ReadEndElement();
				}
				else if (xr.NodeType == XmlNodeType.EndElement)
				{
					var value = converter.Convert("", field.Type);
					field.Property.SetValue(target, value);
					xr.ReadEndElement();
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
				throw new KRUDBackupException(XMLPositionInfo() + "Unknown type: " + typeName);
			}
			return type;
		}

		private string XMLPositionInfo()
		{
			var li = xr as IXmlLineInfo;
			if (li == null) return "";
			return "Line " + li.LineNumber + ", position " + li.LinePosition + ": ";
		}

		void IDisposable.Dispose()
		{
			xr.Dispose();
		}
	}
}
