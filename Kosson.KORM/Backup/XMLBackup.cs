﻿using Kosson.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Kosson.KORM.Backup
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
			return ActivatorUtilities.CreateInstance<XMLBackupReader>(serviceProvider, stream);
		}

		public IBackupWriter CreateWriter(Stream stream)
		{
			return ActivatorUtilities.CreateInstance<XMLBackupWriter>(serviceProvider, stream);
		}
	}
}
