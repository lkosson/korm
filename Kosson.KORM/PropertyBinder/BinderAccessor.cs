namespace Kosson.KORM.PropertyBinder
{
	class BinderAccessor<TInput, TOutput>(IPropertyBinder propertyBinder, IConverter converter, string expression)
	{
		public TOutput Get(TInput input)
		{
			var rawvalue = propertyBinder.Get(input, expression);
			return converter.Convert<TOutput>(rawvalue);
		}
	}
}
