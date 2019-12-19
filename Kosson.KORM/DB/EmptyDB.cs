using System;
using System.Collections;
using System.Data;
using System.Data.Common;

namespace Kosson.KORM.DB
{
	class EmptyDB : ADONETDB
	{
		public override string ConnectionString => "";

		public EmptyDB()
			: base(null)
		{
		}

		protected override DbConnection CreateConnection()
		{
			return new Connection();
		}

		class Connection : DbConnection
		{
			public override string ConnectionString { get; set; }
			public override int ConnectionTimeout => 0;
			public override string Database => "";
			public override ConnectionState State => ConnectionState.Open;
			public override string DataSource => "";
			public override string ServerVersion => "";

			protected override DbTransaction BeginDbTransaction(IsolationLevel il) => new Transaction(this);
			public override void ChangeDatabase(string databaseName) { }
			public override void Close() { }
			protected override DbCommand CreateDbCommand() => new Command(this);
			public override void Open() { }
		}

		class Transaction : DbTransaction
		{
			private DbConnection conn;

			protected override DbConnection DbConnection { get { return conn; } }
			public override IsolationLevel IsolationLevel { get { return System.Data.IsolationLevel.Unspecified; } }

			public Transaction(Connection conn)
			{
				this.conn = conn;
			}

			public override void Commit() { }
			public override void Rollback() { }
		}

		class Command : DbCommand
		{
			public override string CommandText { get; set; }
			public override int CommandTimeout { get; set; }
			public override CommandType CommandType { get; set; }
			protected override DbConnection DbConnection { get; set; }
			protected override DbParameterCollection DbParameterCollection => new Parameters();
			protected override DbTransaction DbTransaction { get; set; }
			public override UpdateRowSource UpdatedRowSource { get; set; }
			public override bool DesignTimeVisible { get; set; }

			public Command(Connection conn)
			{
				DbConnection = conn;
			}

			public override void Cancel() { }

			public override int ExecuteNonQuery() => 0;
			public DbDataReader ExecuteDbReader(CommandBehavior behavior) => new DataReader();
			public override object ExecuteScalar() => null;
			public override void Prepare() { }
			protected override DbParameter CreateDbParameter() => new Parameter();
			protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior) => new DataReader();
		}

		class Parameter : DbParameter
		{
			public override DbType DbType { get; set; }
			public override ParameterDirection Direction { get; set; }
			public override bool IsNullable { get; set; }
			public override string ParameterName { get; set; }
			public override int Size { get; set; }
			public override string SourceColumn { get; set; }
			public override bool SourceColumnNullMapping { get; set; }
			public override object Value { get; set; }
			public override DataRowVersion SourceVersion { get; set; }

			public override void ResetDbType()
			{
			}
		}

		class Parameters : DbParameterCollection
		{
			public override int Count => 0;
			public override object SyncRoot => this;
			public override bool IsFixedSize => false;
			public override bool IsReadOnly => false;
			public override bool IsSynchronized => false;

			public override int Add(object value) => 0;
			public override void AddRange(Array values) { }
			public override void Clear() { }
			public override bool Contains(string value) => false;
			public override bool Contains(object value) => false;
			public override void CopyTo(Array array, int index) { }
			public override IEnumerator GetEnumerator() => new EmptyEnumerator();
			public override int IndexOf(string parameterName) => -1;
			public override int IndexOf(object value) => -1;
			public override void Insert(int index, object value) { }
			public override void Remove(object value) { }
			public override void RemoveAt(string parameterName) { }
			public override void RemoveAt(int index) { }
			protected override DbParameter GetParameter(string parameterName) => null;
			protected override DbParameter GetParameter(int index) => null;
			protected override void SetParameter(string parameterName, DbParameter value) { }
			protected override void SetParameter(int index, DbParameter value) { }
		}

		class DataReader : DbDataReader
		{
			public override object this[string name] => null;
			public override object this[int ordinal] => null;
			public override int Depth => 0;
			public override int FieldCount => 0;
			public override bool HasRows => false;
			public override bool IsClosed => true;
			public override int RecordsAffected => 0;
			public override void Close() { }
			public override bool GetBoolean(int ordinal) => false;
			public override byte GetByte(int ordinal) => 0;
			public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length) => 0;
			public override char GetChar(int ordinal) => '\0';
			public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length) => 0;
			public override string GetDataTypeName(int ordinal) => "";
			public override DateTime GetDateTime(int ordinal) => DateTime.MinValue;
			public override decimal GetDecimal(int ordinal) => 0;
			public override double GetDouble(int ordinal) => 0;
			public override IEnumerator GetEnumerator() => new EmptyEnumerator();
			public override Type GetFieldType(int ordinal) => typeof(object);
			public override float GetFloat(int ordinal) => 0;
			public override Guid GetGuid(int ordinal) => Guid.Empty;
			public override short GetInt16(int ordinal) => 0;
			public override int GetInt32(int ordinal) => 0;
			public override long GetInt64(int ordinal) => 0;
			public override string GetName(int ordinal) => "";
			public override int GetOrdinal(string name) => -1;
			public override string GetString(int ordinal) => null;
			public override object GetValue(int ordinal) => null;
			public override int GetValues(object[] values) => 0;
			public override bool IsDBNull(int ordinal) => true;
			public override bool NextResult() => false;
			public override bool Read() => false;
			public override DataTable GetSchemaTable() => new DataTable();
		}

		class EmptyEnumerator : IEnumerator
		{
			public object Current => null;
			public bool MoveNext() => false;
			public void Reset() { }
		}
	}
}
