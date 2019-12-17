using System.Text;

namespace Kosson.KORM
{
	/// <summary>
	/// Extension methods for Kosson.Interfaces.IRow
	/// </summary>
	public static class IRowExtensions
	{
		/// <summary>
		/// Returns string representation of given row, listing all columns and their values.
		/// </summary>
		/// <param name="row">Row to convert to string.</param>
		/// <returns>String represtatntion of the row.</returns>
		public static string ToStringByColumns(this IRow row)
		{
			var sb = new StringBuilder();
			sb.Append("Row {");
			bool first = true;
			for (int i = 0; i < row.Length; i++)
			{
				if (first)
				{
					first = false;
				}
				else
				{
					sb.Append(", ");
				}
				sb.Append(row.GetName(i) + "=" + row[i]);
			}
			sb.Append(" }");
			return sb.ToString();
		}
	}
}
