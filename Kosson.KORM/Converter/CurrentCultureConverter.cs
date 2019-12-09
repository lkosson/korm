using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kosson.Kore.Converter
{
	/// <summary>
	/// Default implementation of IConverter component using current CultureInfo for string conversions.
	/// </summary>
	public class CurrentCultureConverter : DefaultConverter
	{
		/// <inheritdoc/>
		protected override CultureInfo Culture { get { return CultureInfo.CurrentCulture; } }
	}
}
