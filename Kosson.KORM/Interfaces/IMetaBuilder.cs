using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Kosson.KORM
{
	/// <summary>
	/// Record metadata discovery service.
	/// </summary>
	public interface IMetaBuilder
	{
		/// <summary>
		/// Retrieves DB metadata for a given type.
		/// </summary>
		/// <param name="type">Type to retrieve metadata for.</param>
		/// <returns>Database metadata.</returns>
		IMetaRecord Get(Type type);
	}

	/// <summary>
	/// Metadata describing database record based on a given type.
	/// </summary>
	public interface IMetaRecord : IHasID
	{
		/// <summary>
		/// Type of the record.
		/// </summary>
		Type Type { get; }

		/// <summary>
		/// Type of the record stored in database.
		/// </summary>
		Type? TableType { get; }

		/// <summary>
		/// Determines whether type is marked as a table.
		/// </summary>
		[MemberNotNullWhen(true, nameof(TableType))]
		bool IsTable { get; }

		/// <summary>
		/// Determines whether the values read from database columns of table associated with this class need to be converted to their property types.
		/// </summary>
		bool IsConverted { get; }

		/// <summary>
		/// .NET name of the record type.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Database name of the table backing the record.
		/// </summary>
		string? DBName { get; }

		/// <summary>
		/// Database schema name (if any) of the table backing the record.
		/// </summary>
		string? DBSchema { get; }

		/// <summary>
		/// Prefix for database identifiers used as names for columns backing properties of the record.
		/// </summary>
		string? DBPrefix { get; }

		/// <summary>
		/// Optional custom database query used to retrieve record data.
		/// </summary>
		string? DBQuery { get; }

		/// <summary>
		/// Determines whether primary key (ID) is assigned automatically by database or manually by application.
		/// </summary>
		bool IsManualID { get; }

		/// <summary>
		/// Field of parent record that caused this object to be inlined.
		/// </summary>
		IMetaRecordField? InliningField { get; }

		/// <summary>
		/// 64-bit integer column used for optimistic concurrency control.
		/// </summary>
		IMetaRecordField? RowVersion { get; }

		/// <summary>
		/// Primary key (ID) column/property of the record.
		/// </summary>
		IMetaRecordField PrimaryKey { get; }

		/// <summary>
		/// Columns/properties of the record.
		/// </summary>
		IReadOnlyCollection<IMetaRecordField> Fields { get; }

		/// <summary>
		/// Indices declared for the table used for backing the record.
		/// </summary>
		IReadOnlyCollection<IMetaRecordIndex> Indices { get; }

		/// <summary>
		/// Retrieves column/property of a given name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		IMetaRecordField? GetField(string name);

		internal string GetFieldTableAlias(string name);
	}

	/// <summary>
	/// Metadata describing database column/property of a record.
	/// </summary>
	public interface IMetaRecordField : IHasID
	{
		/// <summary>
		/// Record metadata to which the column/property belongs.
		/// </summary>
		IMetaRecord Record { get; }

		/// <summary>
		/// Property of the record.
		/// </summary>
		PropertyInfo Property { get; }

		/// <summary>
		/// .NET name of the property.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// .NET type of the property.
		/// </summary>
		Type Type { get; }

		/// <summary>
		/// Determines whether a record referenced by this property should be retrieved from database when record containing this property is retrieved.
		/// </summary>
		[MemberNotNullWhen(true, nameof(ForeignMeta))]
		[MemberNotNullWhen(true, nameof(ForeignType))]
		bool IsEagerLookup { get; }

		/// <summary>
		/// Determines whether this property describes a foreign key reference to other record.
		/// </summary>
		[MemberNotNullWhen(true, nameof(ForeignMeta))]
		[MemberNotNullWhen(true, nameof(ForeignType))]
		bool IsRecordRef { get; }

		/// <summary>
		/// Record type of the referenced foreign key table.
		/// </summary>
		Type? ForeignType { get; }

		/// <summary>
		/// Metadata of a referenced foreign key record.
		/// </summary>
		IMetaRecord? ForeignMeta { get; }

		/// <summary>
		/// Determines whether this property is primary key column of a backing database table for containing record.
		/// </summary>
		bool IsPrimaryKey { get; }

		/// <summary>
		/// Determines whether this property is backed by database column and should be stored by ORM.
		/// </summary>
		bool IsColumn { get; }

		/// <summary>
		/// Determines whether this property is backed by database column or subquery and should be retrieved by ORM.
		/// </summary>
		bool IsFromDB { get; }

		/// <summary>
		/// Database name of a column backing this property.
		/// </summary>
		string DBName { get; }

		/// <summary>
		/// Database type of a column backing this property.
		/// </summary>
		DbType DBType { get; }

		/// <summary>
		/// Database engine-specific column definition for backing column.
		/// </summary>
		string? ColumnDefinition { get; }

		/// <summary>
		/// Determines whether column backing this property should not be modified by ORM.
		/// </summary>
		bool IsReadOnly { get; }

		/// <summary>
		/// Maximum data length of a database column backing this property.
		/// </summary>
		int Length { get; }

		/// <summary>
		/// Precision of a data type of database column backing this property.
		/// </summary>
		int Precision { get; }

		/// <summary>
		/// Determines whether value of this property should be truncated to maximum data length declared for backing column.
		/// </summary>
		bool Trim { get; }

		/// <summary>
		/// Determines whether the value read from database needs to be converted to the property type.
		/// </summary>
		bool IsConverted { get; }

		/// <summary>
		/// Determines whether backing column for this property should have generated FOREIGN KEY database constraint.
		/// </summary>
		bool IsForeignKey { get; }

		/// <summary>
		/// Determines whether FOREIGN KEY generated for backing column of this property should have ON DELETE CASCADE clause.
		/// </summary>
		bool IsCascade { get; }

		/// <summary>
		/// Determines whether FOREIGN KEY generated for backing column of this property should have ON DELETE SET NULL clause.
		/// </summary>
		bool IsSetNull { get; }

		/// <summary>
		/// Delegate used for building a subquery to retrieve value for this property.
		/// </summary>
		SubqueryBuilder? SubqueryBuilder { get; }

		/// <summary>
		/// Value used as DEFAULT constraint for database column backing this property.
		/// </summary>
		object? DefaultValue { get; }

		/// <summary>
		/// Determines whether database column backing this property has NOT NULL constraint.
		/// </summary>
		bool IsNotNull { get; }

		/// <summary>
		/// Determines whether properties declared in a type of this property should be treated as though they were declared in this type.
		/// </summary>
		[MemberNotNullWhen(true, nameof(InlineRecord))]
		bool IsInline { get; }

		/// <summary>
		/// Prefix to use for properties declared in type of this property and inlined in this type by ORM.
		/// </summary>
		string? InlinePrefix { get; }

		/// <summary>
		/// Database metadata of a type of this property used for inlining its properties in this type.
		/// </summary>
		IMetaRecord? InlineRecord { get; }
	}

	/// <summary>
	/// Metadata describing database index of a record.
	/// </summary>
	public interface IMetaRecordIndex : IHasID
	{
		/// <summary>
		/// Metadata of a record to which this index belongs.
		/// </summary>
		IMetaRecord Record { get; }

		/// <summary>
		/// Database name of the index.
		/// </summary>
		string DBName { get; }

		/// <summary>
		/// Determines whether index key is unique.
		/// </summary>
		bool IsUnique { get; }

		/// <summary>
		/// Fields of index key.
		/// </summary>
		IReadOnlyCollection<IMetaRecordField> KeyFields { get; }

		/// <summary>
		/// Fields included in covering index leaf nodes.
		/// </summary>
		IReadOnlyCollection<IMetaRecordField> IncludedFields { get; }
	}

	/// <summary>
	/// Delegate used to construct subquery during retrieval of value for a property.
	/// </summary>
	/// <param name="tableAlias">Database alias of a table for record in which the subquery is defined.</param>
	/// <param name="field">Field for which the subquery is defined.</param>
	/// <param name="builder">Command builder to use for subquery building.</param>
	/// <returns>Expression to use as a subquery for a given field.</returns>
	public delegate IDBExpression SubqueryBuilder(string tableAlias, IMetaRecordField field, IDBCommandBuilder builder);
}
