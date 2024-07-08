using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Reflection.Emit;

namespace Kosson.KORM.ORM
{
	class DBParameterLoader
	{
		protected static MethodInfo miInvokeDelegate;
		protected static MethodInfo miVerifyStringParameter;
		protected static Dictionary<Type, object> delegates;

		static DBParameterLoader()
		{
			miInvokeDelegate = typeof(ParameterAdder).GetMethod(nameof(ParameterAdder.Invoke))!;
			miVerifyStringParameter = typeof(DBParameterLoader).GetMethod(nameof(VerifyStringParameter), BindingFlags.NonPublic | BindingFlags.Static)!;
			delegates = [];
		}

		private static string? VerifyStringParameter(string? value, string name, int maxlen, bool trim)
		{
			if (value == null) return null;
			if (value.Length > maxlen)
			{
				if (trim)
					value = value.Substring(0, maxlen);
				else
					throw new KORMDataLengthException(name, maxlen, value);
			}
			return value;
		}

		internal static TDelegate GetOrBuildLoader<TDelegate>(IMetaRecord meta, bool untyped)
		{
			if (delegates.TryGetValue(meta.Type, out var untypedDelegate)) return (TDelegate)untypedDelegate;
			untypedDelegate = BuildLoader(meta, untyped);
			delegates[meta.Type] = untypedDelegate;
			return (TDelegate)untypedDelegate;
		}

		private static object BuildLoader(IMetaRecord meta, bool untyped)
		{
			var delegateArg = untyped ? typeof(object) : meta.Type;
			var delegateType = typeof(ParameterLoader<>).MakeGenericType(delegateArg);

			var dm = new DynamicMethod("Load", null, [typeof(ParameterAdder), delegateArg], true);
			var il = dm.GetILGenerator();

			BuildLoader(il, meta, untyped);

			il.Emit(OpCodes.Ret);
			return dm.CreateDelegate(delegateType);
		}

		private static void BuildLoader(ILGenerator il, IMetaRecord meta, bool untyped)
		{
			// ParameterAdder arg_0;
			// TRecord arg_1;
			// TRecord local = arg_1;
			var local = il.DeclareLocal(meta.Type);
			il.Emit(OpCodes.Ldarg_1);
			if (untyped) il.Emit(OpCodes.Castclass, meta.Type);
			il.Emit(OpCodes.Stloc, local);
			BuildLoader(il, meta, local);
		}

		private static void BuildLoader(ILGenerator il, IMetaRecord meta, LocalBuilder local)
		{
			// ParameterAdder arg_0;
			// TRecord arg_1;
			// TRecordOrInline local;
			foreach (var field in meta.Fields)
			{
				if (!field.IsColumn) continue;
				if (field.IsReadOnly && !field.IsPrimaryKey) continue;

				if (field.IsInline)
				{
					// if (local == null) goto localIsNull;
					var localIsNull = il.DefineLabel();
					il.Emit(OpCodes.Ldloc, local);
					il.Emit(OpCodes.Brfalse, localIsNull);

					// TInline inlinelocal = local.get_field;
					il.Emit(OpCodes.Ldloc, local);
					il.EmitCall(OpCodes.Callvirt, field.Property.GetMethod!, null);
					var inlinelocal = il.DeclareLocal(field.Type);
					il.Emit(OpCodes.Stloc, inlinelocal);

					// localIsNull:
					il.MarkLabel(localIsNull);

					BuildLoader(il, field.InlineRecord!, inlinelocal);
				}
				else
				{
					il.Emit(OpCodes.Ldarg_0); // ST: adder
					// TODO: Somehow replace "@" with proper IDB.CommandBuilder.ParameterPrefix
					il.Emit(OpCodes.Ldstr, "@" + field.DBName); // ST: adder, dbname

					// if (record == null) goto localIsNull;
					var localIsNull = il.DefineLabel();
					var valueLoaded = il.DefineLabel();
					il.Emit(OpCodes.Ldloc, local); // ST: adder, dbname, local
					il.Emit(OpCodes.Brfalse, localIsNull); // ST: adder, dbname

					// __value = local.Get;
					il.Emit(OpCodes.Ldloc, local);  // ST: adder, dbname, record
					il.EmitCall(OpCodes.Callvirt, field.Property.GetMethod!, null); // ST: adder, dbname, value
					if (field.Type.GetTypeInfo().IsValueType) il.Emit(OpCodes.Box, field.Type); // ST: adder, dbname, value

					if (field.Type == typeof(string) && field.Length > 0)
					{
						// __value = VerifyStringParameter(__value, __name, __length, __trim);
						il.Emit(OpCodes.Ldstr, field.Name); // ST: adder, dbname, value, name
						il.Emit(OpCodes.Ldc_I4, field.Length); // ST: adder, dbname, value, name, len
						il.Emit(OpCodes.Ldc_I4, field.Trim ? 1 : 0); // ST: adder, dbname, value, name, len, trim
						il.EmitCall(OpCodes.Call, miVerifyStringParameter, null); // ST: adder, dbname, value
					}

					// goto valueLoaded
					il.Emit(OpCodes.Br_S, valueLoaded); // ST: adder, dbname, value

					// localIsNull:
					il.MarkLabel(localIsNull);
					il.Emit(OpCodes.Ldnull); // ST: adder, dbname, null

					// valueLoaded:
					il.MarkLabel(valueLoaded);

					il.Emit(OpCodes.Ldc_I4, (int)field.DBType); // ST: adder, dbname, value-or-null, dbtype

					il.EmitCall(OpCodes.Call, miInvokeDelegate, null); // ST: 0
				}
			}
		}
		/*
		public static void Run(IDB db, IMetaRecord meta, DbCommand command, object record)
		{
			void AddParameter(string name, object value, DbType dbType)
			{
				var parameter = db.AddParameter(command, name, value);
				parameter.DbType = dbType;
			}

			var loader = GetOrBuildLoader<ParameterLoader<object>>(meta, true);
			loader(AddParameter, record);
		}

		public static void Run(IDB db, IMetaRecord meta, DbBatchCommand command, object record)
		{
			void AddParameter(string name, object value, DbType dbType)
			{
				var parameter = db.AddParameter(command, name, value);
				parameter.DbType = dbType;
			}

			var loader = GetOrBuildLoader<ParameterLoader<object>>(meta, true);
			loader(AddParameter, record);
		}
		*/
		internal delegate void ParameterLoader<TRecord>(ParameterAdder adder, TRecord record);
		internal delegate void ParameterAdder(string name, object value, DbType dbType);
	}

	class DBParameterLoader<TRecord> : DBParameterLoader where TRecord : IRecord
	{
		public static void Run(IDB db, IMetaRecord meta, DbCommand command, TRecord record)
		{
			void AddParameter(string name, object value, DbType dbType)
			{
				var parameter = db.AddParameter(command, name, value);
				parameter.DbType = dbType;
			}

			var loader = GetOrBuildLoader<ParameterLoader<TRecord>>(meta, false);
			loader(AddParameter, record);
		}

		public static void Run(IDB db, IMetaRecord meta, DbBatchCommand command, TRecord record)
		{
			void AddParameter(string name, object value, DbType dbType)
			{
				var parameter = db.AddParameter(command, name, value);
				parameter.DbType = dbType;
			}

			var loader = GetOrBuildLoader<ParameterLoader<TRecord>>(meta, false);
			loader(AddParameter, record);
		}
	}
}
