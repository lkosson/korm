using System;
using System.Collections.Generic;
using System.Text;

namespace Kosson.KORM
{
	public interface ITransaction : IDisposable
	{
		void Commit();
		void Rollback();
	}
}
