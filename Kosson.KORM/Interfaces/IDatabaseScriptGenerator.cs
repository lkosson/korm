using System;
using System.Collections.Generic;
using System.IO;

namespace Kosson.Interfaces
{
	/// <summary>
	/// Interface for generating SQL script for structure and records in the database.
	/// </summary>
	public interface IDatabaseScriptGenerator
	{
		/// <summary>
		/// Writes a SQL script for provided tables.
		/// </summary>
		/// <param name="stream">Stream to write the script to.</param>
		/// <param name="tables">Tables to include in script.</param>
		void GenerateScript(Stream stream, IEnumerable<Type> tables);
	}
}
