using Kosson.Interfaces;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Xml;

namespace Kosson.KORM.MSSQL
{
	/// <summary>
	/// Row based on XML string produced by SQL Server FOR XML query.
	/// </summary>
	public class XMLRow : HashRow
	{
		/// <summary>
		/// Parses provided XML string produced by SQL Server FOR XML query to records of given type.
		/// </summary>
		/// <typeparam name="T">Type of records to create.</typeparam>
		/// <param name="xml">XML string to parse.</param>
		/// <returns>Array of records based on parsed string.</returns>
		public static IReadOnlyCollection<T> Parse<T>(string xml, IConverter converter, IRecordLoader recordLoader, IFactory factory) where T : class, new()
		{
			var prev = CultureInfo.CurrentCulture;
			try
			{
				// for decimal parsing in IConverter
				Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
				var rows = Parse(xml);
				return rows.Load<T>(converter, recordLoader, factory);
			}
			finally
			{
				Thread.CurrentThread.CurrentCulture = prev;
			}
		}

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
