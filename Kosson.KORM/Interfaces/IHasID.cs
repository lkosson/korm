﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kosson.Interfaces
{
	/// <summary>
	/// Object that has ID property suitable for usage as primary key.
	/// </summary>
	public interface IHasID
	{
		/// <summary>
		/// Retrieves primary key value.
		/// </summary>
		long ID { get; }
	}
}
