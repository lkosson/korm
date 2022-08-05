using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace Kosson.KORM
{
	/// <summary>
	/// Provider for creating and restoring XML-based database backups.
	/// </summary>
	public class XMLBackup
	{
		private readonly IServiceProvider serviceProvider;

		/// <summary>
		/// Creates a new XML backup provider.
		/// </summary>
		/// <param name="serviceProvider">Services provider</param>
		public XMLBackup(IServiceProvider serviceProvider)
		{
			this.serviceProvider = serviceProvider;
		}

		/// <summary>
		/// Creates a new reader for restoring backups.
		/// </summary>
		/// <param name="stream">Input stream with a backup.</param>
		/// <returns>Backup reader.</returns>
		public IBackupReader CreateReader(Stream stream)
		{
			return ActivatorUtilities.CreateInstance<Backup.XMLBackupReader>(serviceProvider, stream);
		}

		/// <summary>
		/// Creates a new writer for creating backups.
		/// </summary>
		/// <param name="stream">Output stream to write a backup to.</param>
		/// <returns>Backup writer.</returns>
		public IBackupWriter CreateWriter(Stream stream)
		{
			return ActivatorUtilities.CreateInstance<Backup.XMLBackupWriter>(serviceProvider, stream);
		}
	}
}
