using System;
using System.Linq;

namespace Kosson.KORM
{
	/// <summary>
	/// Base type for IRecord providing equality comparison based on primary key (ID) property.
	/// </summary>
	[Serializable]
	public class Record : Record64
	{
	}
}
