using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kosson.Kore.PropertyBinder
{
	class BinderAccessor<TInput, TOutput>
	{
		private string expression;

		public BinderAccessor(string expression)
		{
			this.expression = expression;
		}

		public TOutput Get(TInput input)
		{
			return input.GetProperty<TOutput>(expression);
		}
	}
}
