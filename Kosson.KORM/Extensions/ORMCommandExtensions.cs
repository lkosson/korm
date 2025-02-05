﻿using Kosson.KORM.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kosson.KORM
{
	/// <summary>
	/// Extension methods for Kosson.Interfaces.IORMCommand.
	/// </summary>
	public static class ORMCommandExtensions
	{
		#region Generic IORMNarrowableCommand extensions
		/// <summary>
		/// Adds WHERE condition based on given SQL condition and parameters. Condition is joined with existing conditions by AND. Parameter names for provided values
		/// are substituted for {0}..{n} placeholders.
		/// </summary>
		/// <typeparam name="TCommand">Type of command to add condition to.</typeparam>
		/// <param name="query">Command or query to add comparison to.</param>
		/// <param name="condition">SQL WHERE condition.</param>
		/// <param name="values">Query parameter values.</param>
		/// <returns>Original command with comparison added to it.</returns>
		public static TCommand WhereRaw<TCommand>(this TCommand query, string condition, params object?[] values)
			where TCommand : IORMNarrowableCommand<TCommand>
		{
			var cb = query.DB.CommandBuilder;
			var parameters = new string[values.Length];
			for (int i = 0; i < values.Length; i++)
			{
				var parameter = query.Parameter(values[i]);
				parameters[i] = parameter.ToString();
			}
			var expr = cb.Expression(String.Format(condition, parameters));
			return query.Where(expr);
		}

		/// <summary>
		/// Adds WHERE condition based on given SQL condition and parameters. Condition is joined with existing conditions by AND.
		/// </summary>
		/// <typeparam name="TCommand">Type of command to add condition to.</typeparam>
		/// <param name="query">Command or query to add comparison to.</param>
		/// <param name="condition">SQL WHERE condition.</param>
		/// <returns>Original command with comparison added to it.</returns>
		public static TCommand Where<TCommand>(this TCommand query, FormattableString condition)
			where TCommand : IORMNarrowableCommand<TCommand>
		{
			var cb = query.DB.CommandBuilder;
			var parameters = new string[condition.ArgumentCount];
			for (int i = 0; i < condition.ArgumentCount; i++)
			{
				var value = condition.GetArgument(i);
				var parameter = query.Parameter(value);
				parameters[i] = parameter.ToString();
			}
			var expr = cb.Expression(String.Format(condition.Format, parameters));
			return query.Where(expr);
		}

		/// <summary>
		/// Adds WHERE condition based on given SQL condition and parameters. Condition is joined with existing conditions by AND. Database field name is substituted for {0}
		/// placeholder. Parameter names for provided values are substituted for {1}..{n+1} placeholders.
		/// </summary>
		/// <typeparam name="TCommand">Type of command to add condition to.</typeparam>
		/// <param name="query">Command or query to add comparison to.</param>
		/// <param name="field">Column or property name for left side of the comparison.</param>
		/// <param name="condition">SQL WHERE condition.</param>
		/// <param name="values">Query parameter values.</param>
		/// <returns>Original command with comparison added to it.</returns>
		public static TCommand WhereField<TCommand>(this TCommand query, string field, string condition, params object?[] values)
			where TCommand : IORMNarrowableCommand<TCommand>
		{
			var cb = query.DB.CommandBuilder;
			var parameters = new string[values.Length + 1];
			parameters[0] = query.Field(field).ToString();
			for (int i = 0; i < values.Length; i++)
			{
				var parameter = query.Parameter(values[i]);
				parameters[i + 1] = parameter.ToString();
			}
			var expr = cb.Expression(String.Format(condition, parameters));
			return query.Where(expr);
		}

		/// <summary>
		/// Adds WHERE condition comparing given column/property and a constant value to the command. Condition is joined with existing conditions by AND.
		/// </summary>
		/// <typeparam name="TCommand">Type of command to add condition to.</typeparam>
		/// <param name="query">Command or query to add comparison to.</param>
		/// <param name="field">Column or property name for left side of the comparison.</param>
		/// <param name="comparison">Comparison type.</param>
		/// <param name="value">Constant value for right side of the comparison.</param>
		/// <returns>Original command with comparison added to it.</returns>
		public static TCommand WhereField<TCommand>(this TCommand query, string field, DBExpressionComparison comparison, object? value)
			where TCommand : IORMNarrowableCommand<TCommand>
		{
			var cb = query.DB.CommandBuilder;
			var pexpr = value == null || value is IRecordRef recordRef && recordRef.ID == 0 ? null : query.Parameter(value);
			var eexpr = cb.Comparison(query.Field(field), comparison, pexpr);
			return query.Where(eexpr);
		}

		/// <summary>
		/// Adds WHERE condition testing for equality between given column/property and a constant value to the command. Condition is joined with existing conditions by AND.
		/// </summary>
		/// <typeparam name="TCommand">Type of command to add condition to.</typeparam>
		/// <param name="query">Command or query to add comparison to.</param>
		/// <param name="field">Column or property name for left side of the comparison.</param>
		/// <param name="value">Constant value for right side of the comparison.</param>
		/// <returns>Original command with comparison added to it.</returns>
		public static TCommand WhereFieldEquals<TCommand>(this TCommand query, string field, object? value)
			where TCommand : IORMNarrowableCommand<TCommand>
			=> WhereField(query, field, DBExpressionComparison.Equal, value);

		/// <summary>
		/// Adds to the command a WHERE condition testing whether given column/property matches provided constant pattern. Condition is joined with existing conditions by AND.
		/// </summary>
		/// <typeparam name="TCommand">Type of command to add condition to.</typeparam>
		/// <param name="query">Command or query to add comparison to.</param>
		/// <param name="field">Column or property name for left side of the comparison.</param>
		/// <param name="value">Constant pattern to check for. Use '_' for matching one character, '%' for matching any number of characters and '[abc]' for matching one of provided characters.</param>
		/// <returns>Original command with comparison added to it.</returns>
		public static TCommand WhereFieldLike<TCommand>(this TCommand query, string field, string value)
			where TCommand : IORMNarrowableCommand<TCommand>
			=> WhereField(query, field, DBExpressionComparison.Like, value);

		/// <summary>
		/// Adds WHERE condition testing if given set contains value of column/property. Condition is joined with existing conditions by AND.
		/// </summary>
		/// <typeparam name="TCommand">Type of command to add condition to.</typeparam>
		/// <typeparam name="TValue">Type of an item in values array.</typeparam>
		/// <param name="query">Command or query to add comparison to.</param>
		/// <param name="field">Column or property name for left side of the comparison.</param>
		/// <param name="values">Values of set for right side of the comparison.</param>
		/// <returns>Original command with comparison added to it.</returns>
		public static TCommand WhereFieldIn<TCommand, TValue>(this TCommand query, string field, params TValue[] values)
			where TCommand : IORMNarrowableCommand<TCommand>
		{
			ArgumentNullException.ThrowIfNull(values);
			if (values.Length == 0) return query.Where(query.DB.CommandBuilder.Const(false));
			if (values.Length == 1 && values[0] is TValue[] nested) values = nested;

			var pexpr = query.Array(values);
			var eexpr = query.DB.CommandBuilder.Comparison(query.Field(field), DBExpressionComparison.In, pexpr);
			return query.Where(eexpr);
		}

		/// <summary>
		/// Adds WHERE condition testing given column/property for NULL to the command. Condition is joined with existing conditions by AND.
		/// </summary>
		/// <typeparam name="TCommand">Type of command to add condition to.</typeparam>
		/// <param name="query">Command or query to add comparison to.</param>
		/// <param name="field">Column or property name to test for NULL.</param>
		/// <returns>Original command with comparison added to it.</returns>
		public static TCommand WhereFieldIsNull<TCommand>(this TCommand query, string field)
			where TCommand : IORMNarrowableCommand<TCommand>
		{
			var cb = query.DB.CommandBuilder;
			var eexpr = cb.IsNull(query.Field(field));
			return query.Where(eexpr);
		}

		/// <summary>
		/// Adds WHERE condition testing given column/property for NOT NULL to the command. Condition is joined with existing conditions by AND.
		/// </summary>
		/// <typeparam name="TCommand">Type of command to add condition to.</typeparam>
		/// <param name="query">Command or query to add comparison to.</param>
		/// <param name="field">Column or property name to test for NOT NULL.</param>
		/// <returns>Original command with comparison added to it.</returns>
		public static TCommand WhereFieldIsNotNull<TCommand>(this TCommand query, string field)
			where TCommand : IORMNarrowableCommand<TCommand>
		{
			var cb = query.DB.CommandBuilder;
			var eexpr = cb.IsNotNull(query.Field(field));
			return query.Where(eexpr);
		}

		/// <summary>
		/// Adds WHERE condition comparing table's primary key to a given constant value. Condition is joined with existing conditions by AND.
		/// </summary>
		/// <typeparam name="TCommand">Type of command to add condition to.</typeparam>
		/// <param name="query">Command or query to add comparison to.</param>
		/// <param name="id">Primary key (ID) value to test for.</param>
		/// <returns>Original command with comparison added to it.</returns>
		public static TCommand WhereID<TCommand>(this TCommand query, long id)
			where TCommand : IORMNarrowableCommand<TCommand>
			=> query.WhereFieldEquals(MetaRecord.PKNAME, id);

		/// <summary>
		/// Adds WHERE condition comparing table's primary key to a result of a subquery with given parameters. 
		/// Parameter names for provided values are substituted for {0}..{n} placeholders.
		/// Condition is joined with existing conditions by AND.
		/// </summary>
		/// <typeparam name="TCommand">Type of command to add condition to.</typeparam>
		/// <param name="query">The query to add condition to.</param>
		/// <param name="subquery">Subquery returning primary key values as a single column.</param>
		/// <param name="values">Subquery parameter values.</param>
		/// <returns>Original command with comparison added to it.</returns>
		public static TCommand WhereIDIn<TCommand>(this TCommand query, string subquery, params object[] values)
			where TCommand : IORMNarrowableCommand<TCommand>
			=> query.WhereField(MetaRecord.PKNAME, "{0} IN (" + String.Format(subquery, Enumerable.Range(1, values.Length).Select(e => "{" + e + "}").ToArray()) + ")", values);

		/// <summary>
		/// Adds WHERE condition comparing table's primary key to an array of values. 
		/// Condition is joined with existing conditions by AND.
		/// </summary>
		/// <typeparam name="TCommand">Type of command to add condition to.</typeparam>
		/// <param name="query">The query to add condition to.</param>
		/// <param name="values">Primary key values to use in condition.</param>
		/// <returns>Original command with comparison added to it.</returns>
		public static TCommand WhereIDIn<TCommand>(this TCommand query, IEnumerable<long> values)
			where TCommand : IORMNarrowableCommand<TCommand>
			=> query.WhereFieldIn(MetaRecord.PKNAME, values.ToArray());

		/// <summary>
		/// Adds a tag to the command.
		/// </summary>
		/// <typeparam name="TCommand">Type of tagged command.</typeparam>
		/// <param name="command">Command to tag.</param>
		/// <param name="comment">Tag comment to add.</param>
		/// <returns>Original command with tag applied</returns>
		public static TCommand Tag<TCommand>(this TCommand command, string comment) where TCommand : IORMCommand<TCommand>
			=> command.Tag(command.DB.CommandBuilder.Comment(comment));
		#endregion
		#region IORMUpdate-specific extensions
		/// <summary>
		/// Executes UPDATE command after adding to it equality comparison between primary key (ID) and given constant value.
		/// Throws ORMException when execution does not modify exactly one database row.
		/// </summary>
		/// <typeparam name="TRecord">Type of record to update.</typeparam>
		/// <param name="query">UPDATE command to execute.</param>
		/// <param name="id">Primary key (ID) of a record to update.</param>
		public static void ByID<TRecord>(this IORMUpdate<TRecord> query, long id) where TRecord : IRecord
		{
			int count = query.WhereID(id).Execute();
			if (count != 1) throw new KORMUpdateFailedException();
		}

		/// <summary>
		/// Executes UPDATE command after adding to it equality comparison between primary key (ID) and given constant value.
		/// Throws ORMException when execution does not modify exactly one database row.
		/// </summary>
		/// <typeparam name="TRecord">Type of record to update.</typeparam>
		/// <param name="query">UPDATE command to execute.</param>
		/// <param name="recordRef">Reference to a record to update.</param>
		public static void ByRef<TRecord>(this IORMUpdate<TRecord> query, RecordRef<TRecord> recordRef) where TRecord : IRecord
			=> ByID(query, recordRef.ID);

		/// <summary>
		/// Asynchronous version of ByID.
		/// Executes UPDATE command after adding to it equality comparison between primary key (ID) and given constant value.
		/// Throws ORMException when execution does not modify exactly one database row.
		/// </summary>
		/// <typeparam name="TRecord">Type of record to update.</typeparam>
		/// <param name="query">UPDATE command to execute.</param>
		/// <param name="id">Primary key (ID) of a record to update.</param>
		public async static Task ByIDAsync<TRecord>(this IORMUpdate<TRecord> query, long id) where TRecord : IRecord
		{
			int count = await query.WhereID(id).ExecuteAsync();
			if (count != 1) throw new KORMUpdateFailedException();
		}

		/// <summary>
		/// Asynchronous version of ByRef.
		/// Executes UPDATE command after adding to it equality comparison between primary key (ID) and given constant value.
		/// Throws ORMException when execution does not modify exactly one database row.
		/// </summary>
		/// <typeparam name="TRecord">Type of record to update.</typeparam>
		/// <param name="query">UPDATE command to execute.</param>
		/// <param name="recordRef">Reference to a record to update.</param>
		public static Task ByRefAsync<TRecord>(this IORMUpdate<TRecord> query, RecordRef<TRecord> recordRef) where TRecord : IRecord
			=> ByIDAsync(query, recordRef.ID);

		/// <summary>
		/// Adds SET clause to the UPDATE command. If the command already contains SET clause, additional assignment is added to it.
		/// </summary>
		/// <typeparam name="TRecord">Type of record on which the command is based.</typeparam>
		/// <param name="query">UPDATE command to modify.</param>
		/// <param name="field">Column or property name to set value of.</param>
		/// <param name="value">Value to set.</param>
		/// <returns>Original command with SET clause added to it.</returns>
		public static IORMUpdate<TRecord> Set<TRecord>(this IORMUpdate<TRecord> query, string field, object? value) where TRecord : IRecord
		{
			var fieldExpr = query.Field(field);
			var valExpr = query.Parameter(value);
			return query.Set(fieldExpr, valExpr);
		}
		#endregion
		#region IORMDelete-specific extensions
		/// <summary>
		/// Executes DELETE command after adding to it equality comparison between primary key (ID) and given constant value.
		/// Throws ORMException when execution does not delete exactly one database row.
		/// </summary>
		/// <typeparam name="TRecord">Type of record to delete.</typeparam>
		/// <param name="query">DELETE command to execute.</param>
		/// <param name="id">Primary key (ID) of a record to delete.</param>
		public static void ByID<TRecord>(this IORMDelete<TRecord> query, long id) where TRecord : IRecord
		{
			int count = query.WhereID(id).Execute();
			if (count != 1) throw new KORMDeleteFailedException();
		}

		/// <summary>
		/// Executes DELETE command after adding to it equality comparison between primary key (ID) and given constant value.
		/// Throws ORMException when execution does not delete exactly one database row.
		/// </summary>
		/// <typeparam name="TRecord">Type of record to delete.</typeparam>
		/// <param name="query">DELETE command to execute.</param>
		/// <param name="recordRef">Reference to a record to delete.</param>
		public static void ByRef<TRecord>(this IORMDelete<TRecord> query, RecordRef<TRecord> recordRef) where TRecord : IRecord
			=> ByID(query, recordRef.ID);

		/// <summary>
		/// Asynchronous version of ByID.
		/// Executes DELETE command after adding to it equality comparison between primary key (ID) and given constant value.
		/// Throws ORMException when execution does not delete exactly one database row.
		/// </summary>
		/// <typeparam name="TRecord">Type of record to delete.</typeparam>
		/// <param name="query">DELETE command to execute.</param>
		/// <param name="id">Primary key (ID) of a record to delete.</param>
		public async static Task ByIDAsync<TRecord>(this IORMDelete<TRecord> query, long id) where TRecord : IRecord
		{
			int count = await query.WhereID(id).ExecuteAsync();
			if (count != 1) throw new KORMDeleteFailedException();
		}

		/// <summary>
		/// Asynchronous version of ByRef.
		/// Executes DELETE command after adding to it equality comparison between primary key (ID) and given constant value.
		/// Throws ORMException when execution does not delete exactly one database row.
		/// </summary>
		/// <typeparam name="TRecord">Type of record to delete.</typeparam>
		/// <param name="query">DELETE command to execute.</param>
		/// <param name="recordRef">Reference to a record to delete.</param>
		public static Task ByRefAsync<TRecord>(this IORMDelete<TRecord> query, RecordRef<TRecord> recordRef) where TRecord : IRecord
			=> ByIDAsync(query, recordRef.ID);
		#endregion
		#region IORMSelect-specific extensions
		/// <summary>
		/// Executes SELECT command after adding to it equality comparison between primary key (ID) and given constant value.
		/// Returns single record with a given ID value or null when no such record is found.
		/// </summary>
		/// <typeparam name="TRecord">Type of record to return.</typeparam>
		/// <param name="query">SELECT command to execute.</param>
		/// <param name="id">Primary KEY (ID) of a record to select.</param>
		/// <returns>Record returned by the query or null if no record matches the query condition.</returns>
		public static TRecord? ByID<TRecord>(this IORMSelect<TRecord> query, long id) where TRecord : IRecord
			=> query.WhereID(id).ExecuteFirst();

		/// <summary>
		/// Executes SELECT command after adding to it equality comparison between primary key (ID) and given set of constant values.
		/// Returns any matching records or an empty collection.
		/// </summary>
		/// <typeparam name="TRecord">Type of record to return.</typeparam>
		/// <param name="query">SELECT command to execute.</param>
		/// <param name="ids">Primary KEY (ID) values of the records to select.</param>
		/// <returns>Records with matching primary key values or empty collection.</returns>
		public static IReadOnlyCollection<TRecord> ByIDs<TRecord>(this IORMSelect<TRecord> query, IEnumerable<long> ids) where TRecord : IRecord
		{
			var usableParameters = query.DB.CommandBuilder.MaxParameterCount * 9 / 10;
			var countIDs = ids.Count();
			if (countIDs <= usableParameters) return query.WhereIDIn(ids).Execute();

			var countDB = query.ExecuteCount();
			if (countDB < countIDs * 10)
			{
				var allRecords = query.Execute();
				var idSet = new HashSet<long>(ids);
				return allRecords.Where(id => idSet.Contains(id.ID)).ToList();
			}

			var result = new List<TRecord>();
			var idBlock = new List<long>(usableParameters);
			foreach (var id in ids)
			{
				idBlock.Add(id);
				if (idBlock.Count >= usableParameters)
				{
					var blockResult = query.Clone().WhereIDIn(idBlock).Execute();
					result.AddRange(blockResult);
					idBlock.Clear();
				}
			}
			if (idBlock.Count > 0)
			{
				var blockResult = query.Clone().WhereIDIn(idBlock).Execute();
				result.AddRange(blockResult);
			}
			return result;
		}

		/// <summary>
		/// Executes SELECT command after adding to it equality comparison between primary key (ID) and given constant value.
		/// Returns single record with a given ID value or null when no such record is found.
		/// </summary>
		/// <typeparam name="TRecord">Type of record to return.</typeparam>
		/// <param name="query">SELECT command to execute.</param>
		/// <param name="recordRef">Reference to a record to select.</param>
		/// <returns>Record returned by the query or null if no record matches the query condition.</returns>
		public static TRecord? ByRef<TRecord>(this IORMSelect<TRecord> query, RecordRef<TRecord> recordRef) where TRecord : IRecord
			=> ByID(query, recordRef.ID);

		/// <summary>
		/// Executes SELECT command after adding to it equality comparison between primary key (ID) and given set of constant values.
		/// Returns any matching records or an empty collection.
		/// </summary>
		/// <typeparam name="TRecord">Type of record to return.</typeparam>
		/// <param name="query">SELECT command to execute.</param>
		/// <param name="refs">Primary KEY (ID) references of the records to select.</param>
		/// <returns>Records with matching primary key values or empty collection.</returns>
		public static IReadOnlyCollection<TRecord> ByRefs<TRecord>(this IORMSelect<TRecord> query, IEnumerable<RecordRef<TRecord>> refs) where TRecord : IRecord
			=> ByIDs(query, refs.Select(r => r.ID));

		/// <summary>
		/// Asynchronous version of ByID.
		/// Executes SELECT command after adding to it equality comparison between primary key (ID) and given constant value.
		/// Returns single record with a given ID value or null when no such record is found.
		/// </summary>
		/// <typeparam name="TRecord">Type of record to return.</typeparam>
		/// <param name="query">SELECT command to execute.</param>
		/// <param name="id">Primary KEY (ID) of a record to select.</param>
		/// <returns>Task representing asynchronous operation returning record returned by the query or null if no record matches the query condition.</returns>
		public static Task<TRecord?> ByIDAsync<TRecord>(this IORMSelect<TRecord> query, long id) where TRecord : IRecord
			=> query.WhereID(id).ExecuteFirstAsync();

		/// <summary>
		/// Asynchronous version of ByIDs.
		/// Executes SELECT command after adding to it equality comparison between primary key (ID) and given set of constant values.
		/// Returns any matching records or an empty collection.
		/// </summary>
		/// <typeparam name="TRecord">Type of record to return.</typeparam>
		/// <param name="query">SELECT command to execute.</param>
		/// <param name="ids">Primary KEY (ID) values of the records to select.</param>
		/// <returns>Records with matching primary key values or empty collection.</returns>
		public static async Task<IReadOnlyCollection<TRecord>> ByIDsAsync<TRecord>(this IORMSelect<TRecord> query, IEnumerable<long> ids) where TRecord : IRecord
		{
			var usableParameters = query.DB.CommandBuilder.MaxParameterCount;
			var countIDs = ids.Count();
			if (countIDs <= usableParameters) return await query.WhereIDIn(ids).ExecuteAsync();

			var countDB = await query.ExecuteCountAsync();
			if (countDB < countIDs * 10)
			{
				var allRecords = await query.ExecuteAsync();
				var idSet = new HashSet<long>(ids);
				return allRecords.Where(id => idSet.Contains(id.ID)).ToList();
			}

			var result = new List<TRecord>();
			var idBlock = new List<long>(usableParameters);
			foreach (var id in ids)
			{
				idBlock.Add(id);
				if (idBlock.Count >= usableParameters)
				{
					var blockResult = await query.Clone().WhereIDIn(idBlock).ExecuteAsync();
					result.AddRange(blockResult);
					idBlock.Clear();
				}
			}
			if (idBlock.Count > 0)
			{
				var blockResult = await query.Clone().WhereIDIn(idBlock).ExecuteAsync();
				result.AddRange(blockResult);
			}
			return result;
		}

		/// <summary>
		/// Asynchronous version of ByRef.
		/// Executes SELECT command after adding to it equality comparison between primary key (ID) and given constant value.
		/// Returns single record with a given ID value or null when no such record is found.
		/// </summary>
		/// <typeparam name="TRecord">Type of record to return.</typeparam>
		/// <param name="query">SELECT command to execute.</param>
		/// <param name="recordRef">Reference to a record to select.</param>
		/// <returns>Task representing asynchronous operation returning record returned by the query or null if no record matches the query condition.</returns>
		public static Task<TRecord?> ByRefAsync<TRecord>(this IORMSelect<TRecord> query, RecordRef<TRecord> recordRef) where TRecord : IRecord
			=> ByIDAsync(query, recordRef.ID);

		/// <summary>
		/// Asynchronous version of ByRefs.
		/// Executes SELECT command after adding to it equality comparison between primary key (ID) and given set of constant values.
		/// Returns any matching records or an empty collection.
		/// </summary>
		/// <typeparam name="TRecord">Type of record to return.</typeparam>
		/// <param name="query">SELECT command to execute.</param>
		/// <param name="refs">Primary KEY (ID) references of the records to select.</param>
		/// <returns>Records with matching primary key values or empty collection.</returns>
		public static Task<IReadOnlyCollection<TRecord>> ByRefsAsync<TRecord>(this IORMSelect<TRecord> query, IEnumerable<RecordRef<TRecord>> refs) where TRecord : IRecord
			=> ByIDsAsync(query, refs.Select(r => r.ID));

		/// <summary>
		/// Executes SELECT command and returns first record of the result or null when resultset is empty.
		/// </summary>
		/// <typeparam name="TRecord">Type of record to return.</typeparam>
		/// <param name="query">SELECT command to execute.</param>
		/// <returns>Record returned by the query or null if empty resultset is returned.</returns>
		public static TRecord? ExecuteFirst<TRecord>(this IORMSelect<TRecord> query) where TRecord : IRecord
		{
			using var reader = query.Limit(1).ExecuteReader();
			if (!reader.MoveNext()) return default;
			return reader.Read();
		}

		/// <summary>
		/// Asynchronous version of ExecuteFirst.
		/// Executes SELECT command and returns first record of the result or null when resultset is empty.
		/// </summary>
		/// <typeparam name="TRecord">Type of record to return.</typeparam>
		/// <param name="query">SELECT command to execute.</param>
		/// <returns>Record returned by the query or null if empty resultset is returned.</returns>
		public async static Task<TRecord?> ExecuteFirstAsync<TRecord>(this IORMSelect<TRecord> query) where TRecord : IRecord
		{
			using var reader = await query.Limit(1).ExecuteReaderAsync();
			if (!await reader.MoveNextAsync()) return default;
			return reader.Read();
		}

		/// <summary>
		/// Executes SELECT command and returns first record of the result or null when resultset is empty.
		/// </summary>
		/// <typeparam name="TRecord">Type of original record.</typeparam>
		/// <typeparam name="TResult">Type of result row.</typeparam>
		/// <param name="query">SELECT command to execute.</param>
		/// <returns>Record returned by the query or null if empty resultset is returned.</returns>
		public static TResult? ExecuteFirst<TRecord, TResult>(this IORMSelectAnonymous<TRecord, TResult> query) where TRecord : IRecord
		{
			//return query.Limit(1).Execute().FirstOrDefault();
			query.OriginalSelect.Limit(1);
			using var reader = query.ExecuteReader();
			if (!reader.MoveNext()) return default;
			return reader.Read();
		}

		/// <summary>
		/// Asynchronous version of ExecuteFirst.
		/// Executes SELECT command and returns first record of the result or null when resultset is empty.
		/// </summary>
		/// <typeparam name="TRecord">Type of original record.</typeparam>
		/// <typeparam name="TResult">Type of result row.</typeparam>
		/// <param name="query">SELECT command to execute.</param>
		/// <returns>Record returned by the query or null if empty resultset is returned.</returns>
		public async static Task<TResult?> ExecuteFirstAsync<TRecord, TResult>(this IORMSelectAnonymous<TRecord, TResult> query) where TRecord : IRecord
		{
			query.OriginalSelect.Limit(1);
			using var reader = await query.ExecuteReaderAsync();
			if (!await reader.MoveNextAsync()) return default;
			return reader.Read();
		}

		/// <summary>
		/// Adds LIMIT 1 clause to the SELECT command, limiting result to a single row.
		/// </summary>
		/// <typeparam name="TRecord">Type of record on which the query is based.</typeparam>
		/// <param name="query">SELECT command to modify.</param>
		/// <returns>Original command with LIMIT clause added to it.</returns>
		public static IORMSelect<TRecord> First<TRecord>(this IORMSelect<TRecord> query) where TRecord : IRecord
			=> query.Limit(1);

		/// <summary>
		/// Adds ORDER BY clause to the SELECT command. If the command already contains ORDER BY clause, additional column is added to it.
		/// </summary>
		/// <typeparam name="TRecord">Type of record on which the query is based.</typeparam>
		/// <param name="query">SELECT command to modify.</param>
		/// <param name="field">Column or property name to order the resultset by.</param>
		/// <returns>Original command with ORDER BY clause added to it.</returns>
		public static IORMSelect<TRecord> OrderBy<TRecord>(this IORMSelect<TRecord> query, string field) where TRecord : IRecord
			=> query.OrderBy(query.Field(field));

		/// <summary>
		/// Adds ORDER BY DESC clause to the SELECT command. If the command already contains ORDER BY clause, additional column is added to it.
		/// </summary>
		/// <typeparam name="TRecord">Type of record on which the query is based.</typeparam>
		/// <param name="query">SELECT command to modify.</param>
		/// <param name="field">Column or property name to order the resultset by.</param>
		/// <returns>Original command with ORDER BY DESC clause added to it.</returns>
		public static IORMSelect<TRecord> OrderByDescending<TRecord>(this IORMSelect<TRecord> query, string field) where TRecord : IRecord
			=> query.OrderBy(query.Field(field), true);
		#endregion
	}
}
