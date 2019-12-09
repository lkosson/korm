﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kosson.Interfaces
{
	/// <summary>
	/// Record containing 64-bit integer representing a row version for optimistic concurrency.
	/// </summary>
	public interface IRecordWithRowVersion
	{
		/// <summary>
		/// Gets or sets a version number of a row backing this record.
		/// </summary>
		long RowVersion { get; set; }
	}

	class IRecordWithRowVersionINT
	{
		public const string NAME = "RowVersion";
	}
}
