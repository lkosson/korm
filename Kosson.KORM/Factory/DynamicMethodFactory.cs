using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Kosson.KORM.Factory
{
	/// <summary>
	/// Implementation of IFactory using Dynamic Methods to create instances of classes.
	/// </summary>
	class DynamicMethodFactory : IFactory
	{
		private readonly Dictionary<Type, Func<object>> constructors = new Dictionary<Type, Func<object>>();

		Func<object> IFactory.GetConstructor(Type type)
		{
			if (!constructors.TryGetValue(type, out var constructor))
			{
				// Value-types factory returns boxed reference, so it cannot be Func<int>, because it's not castable to Func<object>
				var returnType = type.IsValueType ? typeof(object) : type;
				var dm = new DynamicMethod("ctor", returnType, null, true);
				var il = dm.GetILGenerator();
				if (type.IsValueType)
				{
					var loc = il.DeclareLocal(type);
					il.Emit(OpCodes.Ldloca, loc);
					il.Emit(OpCodes.Initobj, type);
					il.Emit(OpCodes.Ldloc, loc);
					il.Emit(OpCodes.Box, type);
				}
				else
				{
					var typeconstr = type.GetConstructor(Type.EmptyTypes) ?? throw new MissingMethodException("Missing default constructor in type " + type + ".");
					il.Emit(OpCodes.Newobj, typeconstr);
				}
				il.Emit(OpCodes.Ret);
				var funcType = typeof(Func<>).MakeGenericType(returnType);
				constructor = (Func<object>)dm.CreateDelegate(funcType);
				constructors[type] = constructor;
			}

			return constructor;
		}
	}
}
