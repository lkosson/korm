using Kosson.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Kosson.KRUD.ORM
{
	class ExecuteProxyBuilder
	{
		protected static MethodInfo miExecute = typeof(ExecuteProxyBase).GetMethod("Execute", BindingFlags.NonPublic | BindingFlags.Static);
		protected static MethodInfo gmiWrapResultArray = typeof(ExecuteProxyBase).GetMethod("WrapResultArray", BindingFlags.NonPublic | BindingFlags.Static);
		protected static MethodInfo gmiWrapResultScalar = typeof(ExecuteProxyBase).GetMethod("WrapResultScalar", BindingFlags.NonPublic | BindingFlags.Static);
		protected static MethodInfo gmiWrapResultSingle = typeof(ExecuteProxyBase).GetMethod("WrapResultSingle", BindingFlags.NonPublic | BindingFlags.Static);
		protected static object syncroot = new object();
		protected static Type typeDict = typeof(Dictionary<string, object>);
		protected static MethodInfo miAdd = typeDict.GetMethod("Add");
	}

	class ExecuteProxyBuilder<TDelegate> : ExecuteProxyBuilder
		where TDelegate : class
	{
		private static TDelegate impl;

		public static TDelegate Get()
		{
			lock (syncroot)
			{
				if (impl == null) BuildMethod();
			}
			return impl;
		}

		private static void BuildMethod()
		{
			MethodInfo method = typeof(TDelegate).GetMethod("Invoke");
			var parameters = method.GetParameters();
			var args = new Type[parameters.Length];
			for (int i = 0; i < args.Length; i++) args[i] = parameters[i].ParameterType;

			var dm = new DynamicMethod("Call", method.ReturnType, args, true);
			var il = dm.GetILGenerator();

			var dbname = typeof(TDelegate).Name;
			var dbnameattribute = (DBNameAttribute)typeof(TDelegate).GetTypeInfo().GetCustomAttribute(typeof(DBNameAttribute), false);
			if (dbnameattribute != null) dbname = dbnameattribute.Name;

			var locDict = il.DeclareLocal(typeDict);
			il.Emit(OpCodes.Newobj, typeDict.GetConstructor(Type.EmptyTypes));
			il.Emit(OpCodes.Stloc, locDict);

			for (int i = 0; i < parameters.Length; i++)
			{
				var parameter = parameters[i];
				var pardbname = parameter.Name;
				var pardbnameattribute = (DBNameAttribute)parameter.GetCustomAttribute(typeof(DBNameAttribute));
				if (pardbnameattribute != null) pardbname = pardbnameattribute.Name;
			
				il.Emit(OpCodes.Ldloc, locDict);
				il.Emit(OpCodes.Ldstr, pardbname);
				il.Emit(OpCodes.Ldarg, i);
				il.EmitCall(OpCodes.Callvirt, miAdd, null);
			}

			il.Emit(OpCodes.Ldstr, dbname);
			il.Emit(OpCodes.Ldloc, locDict);
			il.EmitCall(OpCodes.Call, miExecute, null);
			if (method.ReturnType == typeof(void))
			{
				il.Emit(OpCodes.Pop);
			}
			else if (method.ReturnType.IsArray)
			{
				var miWrapResult = gmiWrapResultArray.MakeGenericMethod(method.ReturnType.GetElementType());
				il.EmitCall(OpCodes.Call, miWrapResult, null);
			}
			else if (method.ReturnType.GetTypeInfo().IsValueType || method.ReturnType == typeof(string))
			{
				var miWrapResult = gmiWrapResultScalar.MakeGenericMethod(method.ReturnType);
				il.EmitCall(OpCodes.Call, miWrapResult, null);
			}
			else
			{
				var miWrapResult = gmiWrapResultSingle.MakeGenericMethod(method.ReturnType);
				il.EmitCall(OpCodes.Call, miWrapResult, null);
			}

			il.Emit(OpCodes.Ret);

			impl = (TDelegate)(object)dm.CreateDelegate(typeof(TDelegate));
		}
	}
}
