﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using System.Reflection.Emit;

namespace Kosson.KORM.ORM
{
	class ReaderRecordLoaderBuilder
	{
		private readonly IMetaBuilder metaBuilder;
		private readonly MethodInfo miIsDBNull;
		private readonly MethodInfo miGetBoolean;
		private readonly MethodInfo miGetByte;
		private readonly MethodInfo miGetInt16;
		private readonly MethodInfo miGetInt32;
		private readonly MethodInfo miGetInt64;
		private readonly MethodInfo miGetFloat;
		private readonly MethodInfo miGetDouble;
		private readonly MethodInfo miGetDecimal;
		private readonly MethodInfo miGetDateTime;
		private readonly MethodInfo miGetGuid;
		private readonly MethodInfo miGetString;
		private readonly MethodInfo miGetValue;
		private readonly MethodInfo miConvert;
		private readonly MethodInfo miCreate;
		private readonly MethodInfo miGetType;

		public ReaderRecordLoaderBuilder(IMetaBuilder metaBuilder)
		{
			this.metaBuilder = metaBuilder;
			miIsDBNull = typeof(DbDataReader).GetMethod(nameof(DbDataReader.IsDBNull))!;
			miGetBoolean = typeof(DbDataReader).GetMethod(nameof(DbDataReader.GetBoolean))!;
			miGetByte = typeof(DbDataReader).GetMethod(nameof(DbDataReader.GetByte))!;
			miGetInt16 = typeof(DbDataReader).GetMethod(nameof(DbDataReader.GetInt16))!;
			miGetInt32 = typeof(DbDataReader).GetMethod(nameof(DbDataReader.GetInt32))!;
			miGetInt64 = typeof(DbDataReader).GetMethod(nameof(DbDataReader.GetInt64))!;
			miGetFloat = typeof(DbDataReader).GetMethod(nameof(DbDataReader.GetFloat))!;
			miGetDouble = typeof(DbDataReader).GetMethod(nameof(DbDataReader.GetDouble))!;
			miGetDecimal = typeof(DbDataReader).GetMethod(nameof(DbDataReader.GetDecimal))!;
			miGetDateTime = typeof(DbDataReader).GetMethod(nameof(DbDataReader.GetDateTime))!;
			miGetGuid = typeof(DbDataReader).GetMethod(nameof(DbDataReader.GetGuid))!;
			miGetString = typeof(DbDataReader).GetMethod(nameof(DbDataReader.GetString))!;
			miGetValue = typeof(DbDataReader).GetMethod(nameof(DbDataReader.GetValue))!;
			miConvert = typeof(IConverter).GetMethod(nameof(IConverter.Convert), [typeof(object), typeof(Type)])!;
			miCreate = typeof(FactoryExtensions).GetMethod(nameof(FactoryExtensions.Create), [typeof(IFactory), typeof(Type)])!;
			miGetType = typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle))!;
		}

		public LoaderFromReaderByIndexDelegate<TRecord> Build<TRecord>()
			where TRecord : IRecord
		{
			var meta = metaBuilder.Get(typeof(TRecord));
			var indices = new Dictionary<string, int>();
			int fieldIndex = 0;
			PrepareIndices(meta, "this", ref fieldIndex, indices, new Stack<long>());

			var dm = new DynamicMethod("Load", null, [typeof(TRecord), typeof(DbDataReader), typeof(IConverter), typeof(IFactory)], true);
			var il = dm.GetILGenerator();

			var localRecord = il.DeclareLocal(typeof(TRecord));
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Stloc, localRecord);

			Build(il, meta, "this", indices, localRecord);

			il.Emit(OpCodes.Ret);
			return (LoaderFromReaderByIndexDelegate<TRecord>)dm.CreateDelegate(typeof(LoaderFromReaderByIndexDelegate<TRecord>));
		}

		private void PrepareIndices(IMetaRecord meta, string prefix, ref int fieldIndex, Dictionary<string, int> indices, Stack<long> descentPath)
		{
			PrepareIndices1(meta, prefix, ref fieldIndex, indices, descentPath);
			PrepareIndices2(meta, prefix, ref fieldIndex, indices, descentPath);
		}

		private void PrepareIndices1(IMetaRecord meta, string prefix, ref int fieldIndex, Dictionary<string, int> indices, Stack<long> descentPath)
		{
			// keep order in sync with DBORMSelect.PrepareTemplate and DBSelect.AppendColumns
			foreach (var field in meta.Fields)
			{
				if (!field.IsFromDB) continue;
				if (field.SubqueryBuilder != null) continue;
				var fieldName = prefix + "." + field.Name;

				if (field.IsInline)
				{
					var foreignMeta = metaBuilder.Get(field.Type);
					if (descentPath.Contains(field.ID)) throw new KORMInvalidOperationException("Cyclic inlines detected on " + meta.Name + "." + field.Name + ".");
					descentPath.Push(field.ID);
					PrepareIndices(foreignMeta, fieldName, ref fieldIndex, indices, descentPath);
					descentPath.Pop();
				}
				else
				{
					indices[fieldName] = fieldIndex;
					fieldIndex++;
				}
			}

			foreach (var field in meta.Fields)
			{
				if (!field.IsFromDB) continue;
				if (!field.IsEagerLookup) continue;
				var fieldName = prefix + "." + field.Name;
				var foreignMeta = metaBuilder.Get(field.Type);
				PrepareIndices1(foreignMeta, fieldName, ref fieldIndex, indices, descentPath);
			}
		}

		private void PrepareIndices2(IMetaRecord meta, string prefix, ref int fieldIndex, Dictionary<string, int> indices, Stack<long> descentPath)
		{
			// keep order in sync with DBORMSelect.PrepareTemplate and DBSelect.AppendColumns
			foreach (var field in meta.Fields)
			{
				if (!field.IsFromDB) continue;
				if (field.SubqueryBuilder == null) continue;
				var fieldName = prefix + "." + field.Name;
				indices[fieldName] = fieldIndex;
				fieldIndex++;
			}

			foreach (var field in meta.Fields)
			{
				if (!field.IsFromDB) continue;
				if (!field.IsEagerLookup) continue;
				var fieldName = prefix + "." + field.Name;
				var foreignMeta = metaBuilder.Get(field.Type);
				if (descentPath.Contains(field.ID)) throw new KORMInvalidOperationException("Cyclic eager lookups detected on " + meta.Name + "." + field.Name + ".");
				descentPath.Push(field.ID);
				PrepareIndices2(foreignMeta, fieldName, ref fieldIndex, indices, descentPath);
				descentPath.Pop();
			}
		}

		private void Build(ILGenerator il, IMetaRecord meta, string prefix, Dictionary<string, int> indices, LocalBuilder localRecord)
		{
			// keep order in sync with DBORMSelect.PrepareTemplate
			foreach (var field in meta.Fields)
			{
				if (!field.IsFromDB) continue;
				var fieldName = prefix + "." + field.Name;

				if (field.IsInline || field.IsEagerLookup)
				{
					BuildForeign(il, field, fieldName, indices, localRecord);
				}
				else
				{
					ReadField(il, field, indices[fieldName], localRecord);
				}
			}
		}

		private void BuildForeign(ILGenerator il, IMetaRecordField field, string fieldName, Dictionary<string, int> indices, LocalBuilder localRecord)
		{
			var type = field.Type;
			var labForeignEnd = il.DefineLabel();
			var labForeignNull = il.DefineLabel();
			var foreignRecord = il.DeclareLocal(type);

			if (field.IsEagerLookup)
			{
				il.Emit(OpCodes.Ldarg_1); // ST: reader
				il.Emit(OpCodes.Ldc_I4, indices[fieldName]); // ST: reader, fieldIndex
				il.EmitCall(OpCodes.Callvirt, miIsDBNull, null); // ST: long
				il.Emit(OpCodes.Brtrue, labForeignNull);
			}

			// _tmp = local.(field.Name);
			il.Emit(OpCodes.Ldloc, localRecord); // ST: record
			il.EmitCall(OpCodes.Callvirt, field.Property.GetMethod!, null); // ST: foreign-or-null
			il.Emit(OpCodes.Stloc, foreignRecord); // ST: <0>

			// if (_tmp != null) goto end;
			il.Emit(OpCodes.Ldloc, foreignRecord); // ST: foreign-or-null
			il.Emit(OpCodes.Brtrue_S, labForeignEnd); // ST: <0>

			// _tmp = factory.Create<T>();
			il.Emit(OpCodes.Ldarg_3); // ST: factory
			il.Emit(OpCodes.Ldtoken, type); // ST: factory, <type>
			il.EmitCall(OpCodes.Call, miGetType, null); // ST: factory, type
			il.EmitCall(OpCodes.Call, miCreate, null); // ST: foreign-as-obj
			if (type.GetTypeInfo().IsValueType)
				il.Emit(OpCodes.Unbox_Any, type); // ST: foreign
			else
				il.Emit(OpCodes.Castclass, type); // ST: foreign
			il.Emit(OpCodes.Stloc, foreignRecord); // ST: <0>

			// local.(field.Name) = _tmp
			il.Emit(OpCodes.Ldloc, localRecord); // ST: record
			il.Emit(OpCodes.Ldloc, foreignRecord); // ST: record, foreign
			il.EmitCall(OpCodes.Callvirt, field.Property.SetMethod!, null); // ST: <0>

			// end:
			il.MarkLabel(labForeignEnd); // ST: <0>

			var foreignMeta = metaBuilder.Get(field.Type);
			Build(il, foreignMeta, fieldName, indices, foreignRecord);

			// null:
			il.MarkLabel(labForeignNull); // ST: <0>
		}

		private void ReadField(ILGenerator il, IMetaRecordField field, int fieldIndex, LocalBuilder localRecord)
		{
			if (field.IsEagerLookup) return; // loaded in 2nd pass by BuildForeign
			else if (field.IsConverted) ReadFieldConvert(il, field, fieldIndex, localRecord);
			else if (field.Type == typeof(bool)) ReadFieldPrimitive(il, field, fieldIndex, miGetBoolean, localRecord);
			else if (field.Type == typeof(byte)) ReadFieldPrimitive(il, field, fieldIndex, miGetByte, localRecord);
			else if (field.Type == typeof(short)) ReadFieldPrimitive(il, field, fieldIndex, miGetInt16, localRecord);
			else if (field.Type == typeof(int)) ReadFieldPrimitive(il, field, fieldIndex, miGetInt32, localRecord);
			else if (field.Type == typeof(long)) ReadFieldPrimitive(il, field, fieldIndex, miGetInt64, localRecord);
			else if (field.Type == typeof(float)) ReadFieldPrimitive(il, field, fieldIndex, miGetFloat, localRecord);
			else if (field.Type == typeof(double)) ReadFieldPrimitive(il, field, fieldIndex, miGetDouble, localRecord);
			else if (field.Type == typeof(decimal)) ReadFieldPrimitive(il, field, fieldIndex, miGetDecimal, localRecord);
			else if (field.Type == typeof(DateTime)) ReadFieldPrimitive(il, field, fieldIndex, miGetDateTime, localRecord);
			else if (field.Type == typeof(Guid)) ReadFieldPrimitive(il, field, fieldIndex, miGetGuid, localRecord);
			else if (field.Type == typeof(string)) ReadFieldPrimitive(il, field, fieldIndex, miGetString, localRecord);
			else if (field.IsRecordRef) ReadFieldRecordRef(il, field, fieldIndex, localRecord);
			else if (field.Type.IsEnum) ReadFieldPrimitive(il, field, fieldIndex, miGetInt32, localRecord);
			else ReadFieldConvert(il, field, fieldIndex, localRecord);
		}

		private void ReadFieldPrimitive(ILGenerator il, IMetaRecordField field, int fieldIndex, MethodInfo getter, LocalBuilder localRecord)
		{
			var labIsNull = il.DefineLabel();
			var labSet = il.DefineLabel();
			var localFieldTypedValue = il.DeclareLocal(field.Type);

			// localRecord.(field.name) = reader.IsDBNull(fieldIndex) ? default : reader.(getter)(fieldIndex)
			il.Emit(OpCodes.Ldloc, localRecord); // ST: record
			il.Emit(OpCodes.Ldarg_1); // ST: record, reader
			il.Emit(OpCodes.Ldc_I4, fieldIndex); // ST: record, reader, fieldIndex
			il.EmitCall(OpCodes.Callvirt, miIsDBNull, null); // ST: record, isnull
			il.Emit(OpCodes.Brtrue_S, labIsNull); // ST: record

			il.Emit(OpCodes.Ldarg_1); // ST: record, reader
			il.Emit(OpCodes.Ldc_I4, fieldIndex); // ST: record, reader, fieldIndex
			il.EmitCall(OpCodes.Callvirt, getter, null); // ST: record, value-as-primitive
			il.Emit(OpCodes.Br_S, labSet); // ST: record, value-as-primitive

			il.MarkLabel(labIsNull); // ST: record
			if (field.Type.IsValueType)
			{
				il.Emit(OpCodes.Ldloca, localFieldTypedValue); // ST: record, &local
				il.Emit(OpCodes.Initobj, field.Type); // ST: record
				il.Emit(OpCodes.Ldloc, localFieldTypedValue); // ST: record, default
			}
			else
			{
				il.Emit(OpCodes.Ldnull); // ST: record, null
			}

			il.MarkLabel(labSet);
			il.EmitCall(OpCodes.Callvirt, field.Property.SetMethod!, null); // ST: 0
		}

		private void ReadFieldRecordRef(ILGenerator il, IMetaRecordField field, int fieldIndex, LocalBuilder localRecord)
		{
			var labIsNull = il.DefineLabel();
			var labSet = il.DefineLabel();

			// localRecord.(field.name) = new RecordRef<T>(reader.IsDBNull(fieldIndex) ? 0 : reader.GetInt64(fieldIndex))
			il.Emit(OpCodes.Ldloc, localRecord); // ST: record
			il.Emit(OpCodes.Ldarg_1); // ST: record, reader
			il.Emit(OpCodes.Ldc_I4, fieldIndex); // ST: record, reader, fieldIndex
			il.EmitCall(OpCodes.Callvirt, miIsDBNull, null); // ST: record, isnull
			il.Emit(OpCodes.Brtrue_S, labIsNull); // ST: record

			il.Emit(OpCodes.Ldarg_1); // ST: record, reader
			il.Emit(OpCodes.Ldc_I4, fieldIndex); // ST: record, reader, fieldIndex
			if (field.DBType == System.Data.DbType.Int32)
			{
				il.EmitCall(OpCodes.Callvirt, miGetInt32, null); // ST: record, int
				il.Emit(OpCodes.Conv_I8);
			}
			else if (field.DBType == System.Data.DbType.Int16)
			{
				il.EmitCall(OpCodes.Callvirt, miGetInt16, null); // ST: record, short
				il.Emit(OpCodes.Conv_I8);
			}
			else if (field.DBType == System.Data.DbType.Byte)
			{
				il.EmitCall(OpCodes.Callvirt, miGetByte, null); // ST: record, byte
				il.Emit(OpCodes.Conv_I8);
			}
			else
			{
				il.EmitCall(OpCodes.Callvirt, miGetInt64, null); // ST: record, long
			}
			il.Emit(OpCodes.Br_S, labSet); // ST: record, long

			il.MarkLabel(labIsNull); // ST: record
			il.Emit(OpCodes.Ldc_I8, 0L); // ST: record, 0L

			il.MarkLabel(labSet); // ST: record, long
			il.Emit(OpCodes.Newobj, field.Type.GetConstructor([typeof(long)])!); // ST: record, RecordRef
			il.EmitCall(OpCodes.Callvirt, field.Property.SetMethod!, null); // ST: 0
		}

		private void ReadFieldConvert(ILGenerator il, IMetaRecordField field, int fieldIndex, LocalBuilder localRecord)
		{
			// localRecord.(field.name) = converter.Convert<field.type>(reader.GetValue(fieldIndex))
			var type = field.Type;
			il.Emit(OpCodes.Ldloc, localRecord); // ST: record
			il.Emit(OpCodes.Ldarg_2); // ST: record, converter
			il.Emit(OpCodes.Ldarg_1); // ST: record, converter, reader
			il.Emit(OpCodes.Ldc_I4, fieldIndex); // ST: record, converter, reader, fieldIndex
			il.EmitCall(OpCodes.Callvirt, miGetValue, null); // ST: record, converter, value-as-object
			il.Emit(OpCodes.Ldtoken, type); // ST: record, converter, value-as-object, <type>
			il.EmitCall(OpCodes.Call, miGetType, null); // ST: record, converter, value-as-object, type
			il.EmitCall(OpCodes.Callvirt, miConvert, null); // ST: record, value-converted-object
			if (type.GetTypeInfo().IsValueType)
				il.Emit(OpCodes.Unbox_Any, type); // ST: record, value-converted-unboxed
			else
				il.Emit(OpCodes.Castclass, type); // ST: record, value-converted-casted
			il.EmitCall(OpCodes.Callvirt, field.Property.SetMethod!, null); // ST: 0
		}
	}

	delegate void LoaderFromReaderByIndexDelegate<TRecord>(TRecord record, DbDataReader reader, IConverter converter, IFactory factory);
}
