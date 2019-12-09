using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kosson.Interfaces;
using Kosson.Kontext;
using System.Collections.Generic;

namespace Kosson.KRUD.Tests
{
	[TestClass]
	public abstract partial class ORMTestsBase : TestBase
	{
		protected const int INTMARKER = 54321;
		protected const string STRINGMARKER = "MARKER";
		protected IORM orm;

		protected abstract IEnumerable<Type> Tables();

		[TestInitialize]
		public override void Init()
		{
 			base.Init();
			Context.Current.Add<IMetaBuilder>(new Kosson.KRUD.Meta.ReflectionMetaBuilder());
			Context.Current.Add<IRecordLoader>(new Kosson.KRUD.RecordLoader.DynamicRecordLoader());
			Context.Current.Add<IORM>(new Kosson.KRUD.ORM.DBORM());

			orm = Context.Current.Get<IORM>();
			orm.CreateTables(Tables());
		}
	}

	[Table]
	class MainTestTable : Record
	{
		public const int DEFAULTVALUE = 321;

		public MainTestTable()
		{
			DefaultValue = DEFAULTVALUE;
			NotNullValue = "";
		}

		[Column]
		public int Value { get; set; }

		[Column(IsNotNull = true)]
		public string NotNullValue { get; set; }

		[Column(HasDefaultValue = true)]
		public int DefaultValue { get; set; }

		[Column(8)]
		public string VarLenString { get; set; }

		[Column(System.Data.DbType.String)]
		public DateTime ExplicitType { get; set; }

		public int NonPersistent { get; set; }

		[Column(IsReadOnly = true)]
		public int ReadOnly { get; set; }
	}

	class ExtendedTable : MainTestTable
	{
		[Column]
		public int ExtensionField { get; set; }
	}
}
