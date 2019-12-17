using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

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
