using Kosson.KORM;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Kosson.KORM
{
	public class XMLBackup
	{
		private readonly IServiceProvider serviceProvider;

		public XMLBackup(IServiceProvider serviceProvider)
		{
			this.serviceProvider = serviceProvider;
		}

		public IBackupReader CreateReader(Stream stream)
		{
			return ActivatorUtilities.CreateInstance<Backup.XMLBackupReader>(serviceProvider, stream);
		}

		public IBackupWriter CreateWriter(Stream stream)
		{
			return ActivatorUtilities.CreateInstance<Backup.XMLBackupWriter>(serviceProvider, stream);
		}
	}
}
