using Kosson.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kosson.Interfaces
{
	/// <summary>
	/// Extension methods for Kosson.Interfaces.RecordRef.
	/// </summary>
	public static class RecordRefExtensions
	{
		/// <summary>
		/// Retrieves a record for a given record reference.
		/// </summary>
		/// <typeparam name="T">Type of record to retrieve.</typeparam>
		/// <param name="recordref">Primary key (ID) reference to a record to retrieve.</param>
		/// <returns>Record for a given record reference.</returns>
		public static T Get<T>(this RecordRef<T> recordref) where T : class, IRecord, new()
		{
			return KORMContext.Current.ORM.Select<T>().ByID<T>(recordref.ID);
		}

		/// <summary>
		/// Asynchronous version of Get.
		/// Retrieves a record for a given record reference.
		/// </summary>
		/// <typeparam name="T">Type of record to retrieve.</typeparam>
		/// <param name="recordref">Primary key (ID) reference to a record to retrieve.</param>
		/// <returns>Task representing asynchronous operation returning record for a given record reference.</returns>
		public static Task<T> GetAsync<T>(this RecordRef<T> recordref) where T : class, IRecord, new()
		{
			return KORMContext.Current.ORM.Select<T>().ByIDAsync<T>(recordref.ID);
		}
	}
}
