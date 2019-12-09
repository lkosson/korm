using Kosson.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Kosson.KRUD.ORM
{
	class DBParameterLoader
	{
		protected static MethodInfo miLoadParameter;
		protected static MethodInfo miVerifyStringParameter;
		protected static Dictionary<Type, object> delegates;

		static DBParameterLoader()
		{
			miLoadParameter = typeof(DBParameterLoader).GetMethod("LoadParameter", BindingFlags.NonPublic | BindingFlags.Static);
			miVerifyStringParameter = typeof(DBParameterLoader).GetMethod("VerifyStringParameter", BindingFlags.NonPublic | BindingFlags.Static);
			delegates = new Dictionary<Type, object>();
		}

		private static void LoadParameter(IDB db, DbCommand cmd, string name, object value, int index, ref DbParameter[] parameters, DbType dbType)
		{
			var parameter = parameters[index];
			if (parameter == null)
			{
				parameter = db.AddParameter(cmd, name, value);
				parameter.DbType = dbType;
				parameters[index] = parameter;
			}
			else
			{
				db.SetParameter(parameter, value);
			}
		}

		private static string VerifyStringParameter(object value, string name, int maxlen, bool trim)
		{
			if (value == null) return null;
			string sval;
			if (value is string) 
				sval = (string)value;
			else
				sval = value.ConvertTo<string>();
			if (sval.Length > maxlen)
			{
				if (trim)
					sval = sval.Substring(0, maxlen);
				else
					throw new KRUDDataLengthException(name, maxlen, sval);
			}
			return sval;
		}

		internal static TDelegate GetOrBuildLoader<TDelegate>(IMetaRecord meta, bool untyped)
		{
			object untypedDelegate;
			if (delegates.TryGetValue(meta.Type, out untypedDelegate)) return (TDelegate)untypedDelegate;
			untypedDelegate = BuildLoader(meta, untyped);
			delegates[meta.Type] = untypedDelegate;
			return (TDelegate)untypedDelegate;
		}

		private static object BuildLoader(IMetaRecord meta, bool untyped)
		{
			var delegateArg = untyped ? typeof(object) : meta.Type;
			var delegateType = typeof(ParameterLoader<>).MakeGenericType(delegateArg);

			var dm = new DynamicMethod("Load", null, new[] { typeof(IDB), typeof(DbCommand), delegateArg, typeof(DbParameter[]).MakeByRefType() }, true);
			var il = dm.GetILGenerator();

			BuildLoader(il, meta, untyped);

			il.Emit(OpCodes.Ret);
			return dm.CreateDelegate(delegateType);
		}

		private static void BuildLoader(ILGenerator il, IMetaRecord meta, bool untyped)
		{
			int count = CountNeededParameters(meta);

			var lblParametersNotNull = il.DefineLabel();
			il.Emit(OpCodes.Ldarg_3);
			il.Emit(OpCodes.Ldind_Ref);
			il.Emit(OpCodes.Brtrue, lblParametersNotNull); // if (parameters != null) goto parametersNotNull;

			il.Emit(OpCodes.Ldarg_3);
			il.Emit(OpCodes.Ldc_I4, count);
			il.Emit(OpCodes.Newarr, typeof(DbParameter));
			il.Emit(OpCodes.Stind_Ref); // parameters = new IDbDataParameter[count];

			il.MarkLabel(lblParametersNotNull); // parametersNotNull:

			var local = il.DeclareLocal(meta.Type);
			int index = 0;
			il.Emit(OpCodes.Ldarg_2);
			if (untyped) il.Emit(OpCodes.Castclass, meta.Type);
			il.Emit(OpCodes.Stloc, local);
			BuildLoader(il, meta, local, ref index);
		}

		private static void BuildLoader(ILGenerator il, IMetaRecord meta, LocalBuilder local, ref int index)
		{
			foreach (var field in meta.Fields)
			{
				if (!field.IsColumn) continue;
				if (field.IsReadOnly && !field.IsPrimaryKey) continue;

				if (field.IsInline)
				{
					// if (record == null) goto localIsNull;
					var localIsNull = il.DefineLabel();
					il.Emit(OpCodes.Ldloc, local);
					il.Emit(OpCodes.Brfalse, localIsNull);

					il.Emit(OpCodes.Ldloc, local);
					var miGet = field.Property.GetGetMethod();
					il.EmitCall(OpCodes.Callvirt, miGet, null);
					var inlinelocal = il.DeclareLocal(field.Type);
					il.Emit(OpCodes.Stloc, inlinelocal);

					// localIsNull:
					il.MarkLabel(localIsNull);

					BuildLoader(il, field.InlineRecord, inlinelocal, ref index);
				}
				else
				{
					il.Emit(OpCodes.Ldarg_0); // ST: idb
					il.Emit(OpCodes.Ldarg_1); // ST: idb, cmd
					il.Emit(OpCodes.Ldstr, field.DBName); // ST: idb, cmd, dbname

					// if (record == null) goto localIsNull;
					var localIsNull = il.DefineLabel();
					var valueLoaded = il.DefineLabel();
					il.Emit(OpCodes.Ldloc, local); // ST: idb, cmd, dbname, local
					il.Emit(OpCodes.Brfalse, localIsNull); // ST: idb, cmd, dbname

					// __value = local.Get;
					il.Emit(OpCodes.Ldloc, local);  // ST: idb, cmd, dbname, record
					var miGet = field.Property.GetGetMethod();
					il.EmitCall(OpCodes.Callvirt, miGet, null); // ST: idb, cmd, dbname, value
					if (field.Type.GetTypeInfo().IsValueType) il.Emit(OpCodes.Box, field.Type); // ST: idb, cmd, dbname, value

					if (field.Type == typeof(string) && field.Length > 0)
					{
						il.Emit(OpCodes.Ldstr, field.Name); // ST: idb, cmd, dbname, value, name
						il.Emit(OpCodes.Ldc_I4, field.Length); // ST: idb, cmd, dbname, value, name, len
						il.Emit(OpCodes.Ldc_I4, field.Trim ? 1 : 0); // ST: idb, cmd, dbname, value, name, len, trim
						il.EmitCall(OpCodes.Call, miVerifyStringParameter, null); // ST: idb, cmd, dbname, value
					}

					// goto valueLoaded
					il.Emit(OpCodes.Br_S, valueLoaded); // ST: idb, cmd, dbname, value

					// localIsNull:
					il.MarkLabel(localIsNull);
					il.Emit(OpCodes.Ldnull); // ST: idb, cmd, dbname, null

					// valueLoaded:
					il.MarkLabel(valueLoaded);

					il.Emit(OpCodes.Ldc_I4, index); // ST: idb, cmd, dbname, value-or-null, index
					il.Emit(OpCodes.Ldarg_3); // ST: idb, cmd, dbname, value-or-null, index, parameters
					il.Emit(OpCodes.Ldc_I4, (int)field.DBType); // ST: idb, cmd, dbname, value-or-null, index, parameters, dbtype

					il.EmitCall(OpCodes.Call, miLoadParameter, null); // ST: 0

					index++;
				}
			}
		}

		private static int CountNeededParameters(IMetaRecord meta)
		{
			int count = 0;
			foreach (var field in meta.Fields)
			{
				if (!field.IsColumn) continue;
				if (field.IsReadOnly && !field.IsPrimaryKey) continue;

				if (field.IsInline)
				{
					count += CountNeededParameters(field.InlineRecord);
				}
				else
				{
					count++;
				}
			}
			return count;
		}

		public static void Run(IDB db, IMetaRecord meta, DbCommand command, object record, ref DbParameter[] parameters)
		{
			var loader = GetOrBuildLoader<ParameterLoader<object>>(meta, true);
			loader(db, command, record, ref parameters);
		}

		internal delegate void ParameterLoader<TRecord>(IDB db, DbCommand cmd, TRecord record, ref DbParameter[] parameters);
	}

	class DBParameterLoader<TRecord> : DBParameterLoader where TRecord : IRecord
	{
		public static void Run(IDB db, IMetaRecord meta, DbCommand command, TRecord record, ref DbParameter[] parameters)
		{
			var loader = GetOrBuildLoader<ParameterLoader<TRecord>>(meta, false);
			loader(db, command, record, ref parameters);
		}
	}
}
