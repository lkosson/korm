using Kosson.KORM;
using System;
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
		private ILGenerator il;

		public ReaderRecordLoaderBuilder(IMetaBuilder metaBuilder)
		{
			this.metaBuilder = metaBuilder;
			miIsDBNull = typeof(DbDataReader).GetMethod(nameof(DbDataReader.IsDBNull));
			miGetBoolean = typeof(DbDataReader).GetMethod(nameof(DbDataReader.GetBoolean));
			miGetByte = typeof(DbDataReader).GetMethod(nameof(DbDataReader.GetByte));
			miGetInt16 = typeof(DbDataReader).GetMethod(nameof(DbDataReader.GetInt16));
			miGetInt32 = typeof(DbDataReader).GetMethod(nameof(DbDataReader.GetInt32));
			miGetInt64 = typeof(DbDataReader).GetMethod(nameof(DbDataReader.GetInt64));
			miGetFloat = typeof(DbDataReader).GetMethod(nameof(DbDataReader.GetFloat));
			miGetDouble = typeof(DbDataReader).GetMethod(nameof(DbDataReader.GetDouble));
			miGetDecimal = typeof(DbDataReader).GetMethod(nameof(DbDataReader.GetDecimal));
			miGetDateTime = typeof(DbDataReader).GetMethod(nameof(DbDataReader.GetDateTime));
			miGetGuid = typeof(DbDataReader).GetMethod(nameof(DbDataReader.GetGuid));
			miGetString = typeof(DbDataReader).GetMethod(nameof(DbDataReader.GetString));
			miGetValue = typeof(DbDataReader).GetMethod(nameof(DbDataReader.GetValue));
			miConvert = typeof(IConverter).GetMethod(nameof(IConverter.Convert), new[] { typeof(object), typeof(Type) });
			miCreate = typeof(FactoryExtensions).GetMethod(nameof(FactoryExtensions.Create), new[] { typeof(IFactory), typeof(Type) });
			miGetType = typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle));
		}

		public LoaderFromReaderByIndexDelegate<TRecord> Build<TRecord>()
			where TRecord : IRecord
		{
			var meta = metaBuilder.Get(typeof(TRecord));
			var indices = new Dictionary<string, int>();
			int fieldIndex = 0;
			PrepareIndices(meta, "this", ref fieldIndex, indices);

			var dm = new DynamicMethod("Load", null, new[] { typeof(TRecord), typeof(DbDataReader), typeof(IConverter), typeof(IFactory) }, true);
			il = dm.GetILGenerator();

			var localRecord = il.DeclareLocal(typeof(TRecord));
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Stloc, localRecord);

			Build(meta, "this", indices, localRecord);

			il.Emit(OpCodes.Ret);
			return (LoaderFromReaderByIndexDelegate<TRecord>)dm.CreateDelegate(typeof(LoaderFromReaderByIndexDelegate<TRecord>));
		}

		private void PrepareIndices(IMetaRecord meta, string prefix, ref int fieldIndex, Dictionary<string, int> indices)
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
					PrepareIndices(foreignMeta, fieldName, ref fieldIndex, indices);
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
				PrepareIndices(foreignMeta, fieldName, ref fieldIndex, indices);
			}

			foreach (var field in meta.Fields)
			{
				if (!field.IsFromDB) continue;
				if (field.SubqueryBuilder == null) continue;
				var fieldName = prefix + "." + field.Name;
				indices[fieldName] = fieldIndex;
				fieldIndex++;
			}
		}

		private void Build(IMetaRecord meta, string prefix, Dictionary<string, int> indices, LocalBuilder localRecord)
		{
			// keep order in sync with DBORMSelect.PrepareTemplate
			foreach (var field in meta.Fields)
			{
				if (!field.IsFromDB) continue;
				var fieldName = prefix + "." + field.Name;

				if (field.IsInline || field.IsEagerLookup)
				{
					BuildForeign(field, fieldName, indices, localRecord);
				}
				else
				{
					ReadField(field, indices[fieldName], localRecord);
				}
			}
		}

		private void BuildForeign(IMetaRecordField field, string fieldName, Dictionary<string, int> indices, LocalBuilder localRecord)
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
			il.EmitCall(OpCodes.Callvirt, field.Property.GetMethod, null); // ST: foreign-or-null
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
			il.EmitCall(OpCodes.Callvirt, field.Property.SetMethod, null); // ST: <0>

			// end:
			il.MarkLabel(labForeignEnd); // ST: <0>

			var foreignMeta = metaBuilder.Get(field.Type);
			Build(foreignMeta, fieldName, indices, foreignRecord);

			// null:
			il.MarkLabel(labForeignNull); // ST: <0>
		}

		private void ReadField(IMetaRecordField field, int fieldIndex, LocalBuilder localRecord)
		{
			if (field.Type == typeof(bool)) ReadFieldPrimitive(field, fieldIndex, miGetBoolean, localRecord);
			else if (field.Type == typeof(byte)) ReadFieldPrimitive(field, fieldIndex, miGetByte, localRecord);
			else if (field.Type == typeof(short)) ReadFieldPrimitive(field, fieldIndex, miGetInt16, localRecord);
			else if (field.Type == typeof(int)) ReadFieldPrimitive(field, fieldIndex, miGetInt32, localRecord);
			else if (field.Type == typeof(long)) ReadFieldPrimitive(field, fieldIndex, miGetInt64, localRecord);
			else if (field.Type == typeof(float)) ReadFieldPrimitive(field, fieldIndex, miGetFloat, localRecord);
			else if (field.Type == typeof(double)) ReadFieldPrimitive(field, fieldIndex, miGetDouble, localRecord);
			else if (field.Type == typeof(decimal)) ReadFieldPrimitive(field, fieldIndex, miGetDecimal, localRecord);
			else if (field.Type == typeof(DateTime)) ReadFieldPrimitive(field, fieldIndex, miGetDateTime, localRecord);
			else if (field.Type == typeof(Guid)) ReadFieldPrimitive(field, fieldIndex, miGetGuid, localRecord);
			else if (field.Type == typeof(string)) ReadFieldPrimitive(field, fieldIndex, miGetString, localRecord);
			else if (field.IsRecordRef) ReadFieldRecordRef(field, fieldIndex, localRecord);
			else if (field.IsEagerLookup) return; // loaded in 2nd pass by BuildForeign
			else ReadFieldConvert(field, fieldIndex, localRecord);
		}

		private void ReadFieldPrimitive(IMetaRecordField field, int fieldIndex, MethodInfo getter, LocalBuilder localRecord)
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
			il.EmitCall(OpCodes.Callvirt, field.Property.SetMethod, null); // ST: 0
		}

		private void ReadFieldRecordRef(IMetaRecordField field, int fieldIndex, LocalBuilder localRecord)
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
			il.EmitCall(OpCodes.Callvirt, miGetInt64, null); // ST: record, long
			il.Emit(OpCodes.Br_S, labSet); // ST: record, long

			il.MarkLabel(labIsNull); // ST: record
			il.Emit(OpCodes.Ldc_I8, 0L); // ST: record, 0L

			il.MarkLabel(labSet); // ST: record, long
			il.Emit(OpCodes.Newobj, field.Type.GetConstructor(new[] { typeof(long) })); // ST: record, RecordRef
			il.EmitCall(OpCodes.Callvirt, field.Property.SetMethod, null); // ST: 0
		}

		private void ReadFieldConvert(IMetaRecordField field, int fieldIndex, LocalBuilder localRecord)
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
			il.EmitCall(OpCodes.Callvirt, field.Property.SetMethod, null); // ST: 0
		}
	}

	delegate void LoaderFromReaderByIndexDelegate<TRecord>(TRecord record, DbDataReader reader, IConverter converter, IFactory factory);
}
