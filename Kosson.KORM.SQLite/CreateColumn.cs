using Kosson.KORM.DB.CommandBuilder;
using System.Text;

namespace Kosson.KORM.SQLite
{
	class CreateColumn(IDBCommandBuilder builder) : DBCreateColumn(builder)
	{
		protected override void AppendNotNull(StringBuilder sb)
		{
			// SQLite requires default value for added NOT NULL columns.
			if (notNull && defaultValue == null)
			{
				sb.Append("CHECK (");
				AppendName(sb);
				sb.Append("IS NOT NULL)");
			}
			else
			{
				base.AppendNotNull(sb);
			}
		}
	}
}
