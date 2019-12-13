﻿using Kosson.Interfaces;

namespace Kosson.KORM.PropertyBinder
{
	class BinderAccessor<TInput, TOutput>
	{
		private string expression;
		private IPropertyBinder propertyBinder;
		private IConverter converter;

		public BinderAccessor(IPropertyBinder propertyBinder, IConverter converter, string expression)
		{
			this.expression = expression;
			this.propertyBinder = propertyBinder;
			this.converter = converter;
		}

		public TOutput Get(TInput input)
		{
			var rawvalue = propertyBinder.Get(input, expression);
			return converter.Convert<TOutput>(rawvalue);
		}
	}
}
