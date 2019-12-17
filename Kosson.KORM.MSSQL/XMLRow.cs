using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Kosson.KORM.MSSQL
{
	/// <summary>
	/// Row based on XML string produced by SQL Server FOR XML query.
	/// </summary>
	public class XMLRow : HashRow
	{
		/// <summary>
		/// Parses provided XML string produced by SQL Server FOR XML query to array of IRows.
		/// </summary>
		/// <param name="xml">XML string to parse.</param>
		/// <returns>Array of IRows based on parsed string.</returns>
		public static XMLRow[] Parse(string xml)
		{
			var reader = XmlReader.Create(new StringReader("<root>" + xml + "</root>"));

			reader.ReadStartElement();
			var list = new List<XMLRow>();
			do
			{
				if (!reader.IsStartElement()) continue;
				if (reader.AttributeCount == 0) continue;
				var row = new XMLRow();
				while (reader.MoveToNextAttribute())
				{
					row.Add(reader.Name, reader.Value);
				}
				list.Add(row);
				reader.MoveToElement();
			}
			while (reader.Read());
			return list.ToArray();
		}
	}
}
