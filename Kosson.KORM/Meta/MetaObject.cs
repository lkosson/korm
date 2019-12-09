using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Kosson.KRUD.Meta
{
	class MetaObject
	{
		private static long idgenerator;
		public long ID { get; private set; }

		public MetaObject()
		{
			ID = Interlocked.Increment(ref idgenerator);
		}
	}
}
