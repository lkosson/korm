using Kosson.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kosson.KRUD.ORM
{
	class DBTableCreator
	{
		private IMetaBuilder metaBuilder;
		private IDB db;
		private IDBCommandBuilder cb;
		private Action<IDBCommand> executor;

		public DBTableCreator(IDB db, IMetaBuilder metaBuilder, Action<IDBCommand> customExecutor = null)
		{
			this.db = db;
			this.metaBuilder = metaBuilder;
			cb = db.CommandBuilder;
			executor = customExecutor ?? DefaultExecute;
		}

		private void DefaultExecute(IDBCommand cmd)
		{
			try
			{
				var cmdtext = cmd.ToString();
				if (String.IsNullOrWhiteSpace(cmdtext)) return;
				db.ExecuteNonQuery(cmdtext);
			}
			catch (KRUDObjectExistsException)
			{
				// ignored
			}
		}

		public void Create(IEnumerable<Type> types)
		{
			var metas = types.Select(t => metaBuilder.Get(t)).ToArray();

			foreach (var meta in metas) CreateTable(meta);
			foreach (var meta in metas) CreateColumns(meta);
			foreach (var meta in metas) CreateIndices(meta);
			foreach (var meta in metas) CreateForeignKeys(meta);
		}

		public void Create(IMetaRecord meta)
		{
			CreateTable(meta);
			CreateColumns(meta);
			CreateIndices(meta);
			CreateForeignKeys(meta);
		}

		internal void CreateTable(IMetaRecord meta)
		{
			var table = cb.CreateTable();
			table.Table(cb.Identifier(meta.DBName));
			table.PrimaryKey(cb.Identifier(meta.PrimaryKey.DBName));
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

		private void CreateColumn(IMetaRecord meta, IMetaRecordField field)
		{
			var column = cb.CreateColumn();
			column.Table(cb.Identifier(meta.DBName));
			column.Name(cb.Identifier(field.DBName));
			if (String.IsNullOrEmpty(field.ColumnDefinition))
			{
				column.Type(cb.Type(field.DBType, field.Length, field.Precision));
				if (field.IsNotNull) column.NotNull();
				if (field.DefaultValue != null) column.DefaultValue(cb.Const(field.DefaultValue));
			}
			else
			{
				column.Type(cb.Expression(field.ColumnDefinition));
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
			create.Table(cb.Identifier(meta.DBName));
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
			fk.TargetTable(cb.Identifier(remotemeta.DBName));
			fk.TargetColumn(cb.Identifier(remotemeta.PrimaryKey.DBName));
			if (field.IsCascade) fk.Cascade();
			if (field.IsSetNull) fk.SetNull();
		}

		private void CreateForeignKey(IMetaRecord meta, IMetaRecordField field)
		{
			var fk = cb.CreateForeignKey();
			fk.Table(cb.Identifier(meta.DBName));
			fk.Column(cb.Identifier(field.DBName));
			PrepareForeignKey(fk, field);
			executor(fk);
		}
	}
}
