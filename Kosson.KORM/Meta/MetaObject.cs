using System.Threading;

namespace Kosson.KORM.Meta
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
