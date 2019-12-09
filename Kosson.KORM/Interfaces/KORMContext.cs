using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Kosson.Interfaces
{
	public class KORMContext
	{
		private static AsyncLocal<KORMContext> currentContext = new AsyncLocal<KORMContext>();

		public IORM ORM { get; private set; }
		public IDB DB { get; private set; }
		public IFactory Factory { get; private set; }
		public IConverter Converter { get; private set; }
		public IRecordLoader RecordLoader { get; internal set; }
		public IMetaBuilder MetaBuilder { get; internal set; }
		public IBackupProvider BackupProvider { get; internal set; }
		public IPropertyBinder PropertyBinder { get; internal set; }
		public IConfiguration Configuration { get; internal set; }
		public ILogger Logger { get; internal set; }
		public IDBFactory DBFactory { get; internal set; }
		public bool IsNested { get; internal set; }

		public static KORMContext Current
		{
			get
			{
				KORMContext context = null;
				if (currentContext.Value != null) context = currentContext.Value;

				// context captured for async method/timer can go out-of-scope in original thread and get disposed
				//while (context != null && context.IsDisposed) context = context.Parent;
				return context;
			}

			set
			{
				currentContext.Value = value;
			}
		}

		private KORMContext()
		{
			//var provider = Context.Current.Configuration("db-provider");
			//if (String.IsNullOrEmpty(provider)) throw new ArgumentNullException("\"db-provider\" configuration key not defined.");
			//return KORMContext.Current.DBFactory.Create(provider, false);
		}
	}
}
