using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Kosson.KRUD
{
	class EmptyDB : ADONETDB
	{
		public override string ConnectionString { get { return ""; } }

		public EmptyDB()
			: base(null, null)
		{

		}

		protected override DbConnection CreateConnection()
		{
			return new Connection();
		}

		class Connection : DbConnection
		{
			public override string ConnectionString { get; set; }
			public override int ConnectionTimeout { get { return 0; } }
			public override string Database { get { return ""; } }
			public override ConnectionState State { get { return ConnectionState.Open; } }
			public override string DataSource { get { return ""; } }
			public override string ServerVersion {  get { return ""; } }

			protected override DbTransaction BeginDbTransaction(IsolationLevel il)
			{
				return new Transaction(this);
			}

			public override void ChangeDatabase(string databaseName)
			{
			}

			public override void Close()
			{
			}

			protected override DbCommand CreateDbCommand()
			{
				return new Command(this);
			}

			public override void Open()
			{
			}
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

			public override void Commit()
			{
			}

			public override void Rollback()
			{
			}
		}

		class Command : DbCommand
		{
			public override string CommandText { get; set; }
			public override int CommandTimeout { get; set; }
			public override CommandType CommandType { get; set; }
			protected override DbConnection DbConnection { get; set; }
			protected override DbParameterCollection DbParameterCollection { get { return new Parameters(); } }
			protected override DbTransaction DbTransaction { get; set; }
			public override UpdateRowSource UpdatedRowSource { get; set; }
			public override bool DesignTimeVisible { get; set; }

			public Command(Connection conn)
			{
				DbConnection = conn;
			}

			public override void Cancel()
			{
			}

			public override int ExecuteNonQuery()
			{
				return 0;
			}

			public DbDataReader ExecuteDbReader(CommandBehavior behavior)
			{
				return new DataReader();
			}

			public override object ExecuteScalar()
			{
				return null;
			}

			public override void Prepare()
			{
			}

			protected override DbParameter CreateDbParameter()
			{
				return new Parameter();
			}

			protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
			{
				return new DataReader();
			}
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
			public override int Count { get { return 0; } }

			public override object SyncRoot { get { return this; } }

			public override bool IsFixedSize { get { return false; } }

			public override bool IsReadOnly { get { return false; } }

			public override bool IsSynchronized { get { return false; } }

			public override int Add(object value)
			{
				return 0;
			}

			public override void AddRange(Array values)
			{
			}

			public override void Clear()
			{
			}

			public override bool Contains(string value)
			{
				return false;
			}

			public override bool Contains(object value)
			{
				return false;
			}

			public override void CopyTo(Array array, int index)
			{
			}

			public override IEnumerator GetEnumerator()
			{
				return new EmptyEnumerator();
			}

			public override int IndexOf(string parameterName)
			{
				return -1;
			}

			public override int IndexOf(object value)
			{
				return -1;
			}

			public override void Insert(int index, object value)
			{
			}

			public override void Remove(object value)
			{
			}

			public override void RemoveAt(string parameterName)
			{
			}

			public override void RemoveAt(int index)
			{
			}

			protected override DbParameter GetParameter(string parameterName)
			{
				return null;
			}

			protected override DbParameter GetParameter(int index)
			{
				return null;
			}

			protected override void SetParameter(string parameterName, DbParameter value)
			{
			}

			protected override void SetParameter(int index, DbParameter value)
			{
			}
		}

		class DataReader : DbDataReader
		{
			public override object this[string name] { get { return null; } }

			public override object this[int ordinal] { get { return null; } }

			public override int Depth { get { return 0; } }

			public override int FieldCount {  get { return 0; } }

			public override bool HasRows {  get { return false; } }

			public override bool IsClosed {  get { return true; } }

			public override int RecordsAffected {  get { return 0; } }

			public override void Close()
			{
			}

			public override bool GetBoolean(int ordinal)
			{
				return false;
			}

			public override byte GetByte(int ordinal)
			{
				return 0;
			}

			public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
			{
				return 0;
			}

			public override char GetChar(int ordinal)
			{
				return '\0';
			}

			public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
			{
				return 0;
			}

			public override string GetDataTypeName(int ordinal)
			{
				return "";
			}

			public override DateTime GetDateTime(int ordinal)
			{
				return DateTime.MinValue;
			}

			public override decimal GetDecimal(int ordinal)
			{
				return 0;
			}

			public override double GetDouble(int ordinal)
			{
				return 0;
			}

			public override IEnumerator GetEnumerator()
			{
				return new EmptyEnumerator();
			}

			public override Type GetFieldType(int ordinal)
			{
				return typeof(Object);
			}

			public override float GetFloat(int ordinal)
			{
				return 0;
			}

			public override Guid GetGuid(int ordinal)
			{
				return Guid.Empty;
			}

			public override short GetInt16(int ordinal)
			{
				return 0;
			}

			public override int GetInt32(int ordinal)
			{
				return 0;
			}

			public override long GetInt64(int ordinal)
			{
				return 0;
			}

			public override string GetName(int ordinal)
			{
				return "";
			}

			public override int GetOrdinal(string name)
			{
				return -1;
			}

			public override string GetString(int ordinal)
			{
				return null;
			}

			public override object GetValue(int ordinal)
			{
				return null;
			}

			public override int GetValues(object[] values)
			{
				return 0;
			}

			public override bool IsDBNull(int ordinal)
			{
				return true;
			}

			public override bool NextResult()
			{
				return false;
			}

			public override bool Read()
			{
				return false;
			}

			public override DataTable GetSchemaTable()
			{
				return new DataTable();
			}
		}

		class EmptyEnumerator : IEnumerator
		{
			public object Current { get { return null; } }

			public bool MoveNext()
			{
				return false;
			}

			public void Reset()
			{
			}
		}
	}
}
