using System;
using System.Collections.Generic;
using System.Text;

namespace Kosson.Interfaces
{
	public interface IBackupRestorer
	{
		void Restore(IBackupReader reader);
	}
}
