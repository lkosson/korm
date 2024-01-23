using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Kosson.KORM.ORM
{
	class DBTableCreator
	{
		private IMetaBuilder metaBuilder;
		private IDB db;
		private IDBCommandBuilder cb;
		private Action<IDBCommand> executor;
		private readonly ILogger logger;

		public DBTableCreator(IDB db, IMetaBuilder metaBuilder, ILogger logger, Action<IDBCommand> customExecutor = null)
		{
			this.db = db;
			this.metaBuilder = metaBuilder;
			this.logger = logger;
			cb = db.CommandBuilder;
			executor = customExecutor ?? DefaultExecute;
		}

		private void DefaultExecute(IDBCommand cmd)
		{
			try
			{
				var cmdtext = cmd.ToString();
				if (String.IsNullOrWhiteSpace(cmdtext)) return;
				db.ExecuteNonQueryRaw(cmdtext);
			}
			catch (KORMObjectExistsException)
			{
				// ignored
			}
		}

		public void Create(IEnumerable<Type> types)
		{
			var sw = Stopwatch.StartNew();
			logger?.LogInformation("Create");
			var metas = types.Select(t => metaBuilder.Get(t)).ToArray();
			var schemas = metas.GroupBy(m => m.DBSchema).Where(g => g.Key != null).Select(g => g.First()).ToArray();

			foreach (var meta in schemas) CreateSchema(meta);
			foreach (var meta in metas) CreateTable(meta);
			foreach (var meta in metas) CreateColumns(meta);
			foreach (var meta in metas) CreateIndices(meta);
			foreach (var meta in metas) CreateForeignKeys(meta);
			logger?.LogInformation("Create\t" + sw.ElapsedMilliseconds + " ms\t" + metas.Length);
		}

		public void Create(IMetaRecord meta)
		{
			CreateSchema(meta);
			CreateTable(meta);
			CreateColumns(meta);
			CreateIndices(meta);
			CreateForeignKeys(meta);
		}

		internal void CreateSchema(IMetaRecord meta)
		{
			if (meta.DBSchema == null) return;
			var schema = cb.CreateSchema();
			schema.Schema(cb.Identifier(meta.DBSchema));
			executor(schema);
		}

		internal void CreateTable(IMetaRecord meta)
		{
			var table = cb.CreateTable();
			table.Table(cb.Identifier(meta.DBSchema, meta.DBName));
			table.PrimaryKey(cb.Identifier(meta.PrimaryKey.DBName), CreateColumnTypeExpression(meta.PrimaryKey));
			if (!meta.IsManualID) table.AutoIncrement();
			executor(table);
		}

		internal void CreateColumns(IMetaRecord meta)
		{
			foreach (var field in meta.Fields)
			{
				if (!field.IsColumn) continue;
				if (field.IsPrimaryKey) continue;
				if (field.IsInline)
				{
					CreateColumns(field.InlineRecord);
					continue;
				}
				CreateColumn(meta, field);
			}
		}

		private IDBExpression CreateColumnTypeExpression(IMetaRecordField field)
		{
			if (!String.IsNullOrEmpty(field.ColumnDefinition)) return cb.Expression(field.ColumnDefinition);
			return cb.Type(field.DBType, field.Length, field.Precision);
		}

		private void CreateColumn(IMetaRecord meta, IMetaRecordField field)
		{
			var column = cb.CreateColumn();
			column.Table(cb.Identifier(meta.DBSchema, meta.DBName));
			column.Name(cb.Identifier(field.DBName));
			column.Type(CreateColumnTypeExpression(field));
			if (String.IsNullOrEmpty(field.ColumnDefinition))
			{
				if (field.IsNotNull) column.NotNull();
				if (field.DefaultValue != null) column.DefaultValue(cb.Const(field.DefaultValue is bool boolValue ? boolValue ? 1 : 0 : field.DefaultValue));
			}
			if (field.IsForeignKey) PrepareForeignKey(column, field);
			executor(column);
		}

		internal void CreateIndices(IMetaRecord meta)
		{
			foreach (var index in meta.Indices)
			{
				CreateIndex(meta, index);
			}
		}

		private void CreateIndex(IMetaRecord meta, IMetaRecordIndex index)
		{
			var create = cb.CreateIndex();
			create.Table(cb.Identifier(meta.DBSchema, meta.DBName));
			create.Name(cb.Identifier(String.Format(index.DBName, meta.DBName)));
			if (index.IsUnique) create.Unique();
			foreach (var field in index.KeyFields)
			{
				create.Column(cb.Identifier(field.DBName));
			}
			foreach (var field in index.IncludedFields)
			{
				create.Include(cb.Identifier(field.DBName));
			}
			executor(create);
		}

		internal void CreateForeignKeys(IMetaRecord meta)
		{
			foreach (var field in meta.Fields)
			{
				if (!field.IsColumn) continue;
				if (!field.IsForeignKey) continue;
				CreateForeignKey(meta, field);
			}
		}

		private void PrepareForeignKey(IDBForeignKey fk, IMetaRecordField field)
		{
			var remotemeta = metaBuilder.Get(field.Type);
			fk.ConstraintName(cb.Identifier("FK_" + field.Record.DBName + "_" + field.DBName + "_" + remotemeta.DBName));
			fk.TargetTable(cb.Identifier(remotemeta.DBSchema, remotemeta.DBName));
			fk.TargetColumn(cb.Identifier(remotemeta.PrimaryKey.DBName));
			if (field.IsCascade) fk.Cascade();
			if (field.IsSetNull) fk.SetNull();
		}

		private void CreateForeignKey(IMetaRecord meta, IMetaRecordField field)
		{
			var fk = cb.CreateForeignKey();
			fk.Table(cb.Identifier(meta.DBSchema, meta.DBName));
			fk.Column(cb.Identifier(field.DBName));
			PrepareForeignKey(fk, field);
			executor(fk);
		}
	}
}
