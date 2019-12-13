using System;
using System.Reflection.Emit;

namespace Kosson.KORM.PropertyBinder
{
	class DynamicAccessor
	{
		public static Func<TInput, TOutput> BuildAccesor<TInput, TOutput>(string expression)
		{
			var dm = new DynamicMethod("Accessor-" + expression, typeof(TOutput), new Type[] { typeof(TInput) }, true);
			var il = dm.GetILGenerator();

			var propertyExpr = expression;
			var type = typeof(TInput);
			il.Emit(OpCodes.Ldarg_0);
			do
			{
				var lblNotNull = il.DefineLabel();
				il.Emit(OpCodes.Dup);
				il.Emit(OpCodes.Brtrue, lblNotNull);
				il.Emit(OpCodes.Pop);

				if (typeof(TOutput).IsValueType)
				{
					var result = il.DeclareLocal(typeof(TOutput));
					il.Emit(OpCodes.Ldloca_S, result);
					il.Emit(OpCodes.Initobj, typeof(TOutput));
					il.Emit(OpCodes.Ldloc, result);
				}
				else
				{
					il.Emit(OpCodes.Ldnull);
				}
				il.Emit(OpCodes.Ret);
				il.MarkLabel(lblNotNull);

				var propertyName = propertyExpr;
				var dot = propertyName.IndexOf('.');
				if (dot >= 0)
				{
					propertyExpr = propertyName.Substring(dot + 1);
					propertyName = propertyName.Substring(0, dot);
				}
				else
				{
					propertyExpr = null;
				}

				var property = type.GetProperty(propertyName);
				var propertyGet = property.GetGetMethod();
				type = property.PropertyType;

				il.EmitCall(OpCodes.Callvirt, propertyGet, null);
			}
			while (propertyExpr != null);

			if (!typeof(TOutput).IsAssignableFrom(type)) throw new ArgumentException("Declared output type " + typeof(TOutput) + " doesn't match actual output type " + type + ".");

			if (!typeof(TOutput).IsValueType && type.IsValueType) il.Emit(OpCodes.Box, type);

			il.Emit(OpCodes.Ret);

			return (Func<TInput, TOutput>)dm.CreateDelegate(typeof(Func<TInput, TOutput>));
		}
	}
}
