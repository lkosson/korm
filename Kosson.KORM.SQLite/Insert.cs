using Kosson.KORM.DB.CommandBuilder;

namespace Kosson.KORM.SQLite
{
	class Insert : DBInsert, IDBInsert
	{
		public Insert(IDBCommandBuilder builder)
			: base(builder)
		{
		}

		private Insert(Insert template)
			: base(template)
		{
		}

		public override IDBInsert Clone()
		{
			return new Insert(this);
		}

		string IDBInsert.GetLastID { get { return "SELECT last_insert_rowid() AS ID"; } }
	}
}
