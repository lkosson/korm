using Kosson.Interfaces;
using System;
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
			var dm = new DynamicMethod("Load", null, new[] { typeof(TRecord), typeof(DbDataReader), typeof(IConverter), typeof(IFactory) }, true);
			il = dm.GetILGenerator();
			var meta = metaBuilder.Get(typeof(TRecord));

			var localRecord = il.DeclareLocal(typeof(TRecord));
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Stloc, localRecord);

			int fieldIndex = 0;
			Build(meta, ref fieldIndex, localRecord);

			il.Emit(OpCodes.Ret);
			return (LoaderFromReaderByIndexDelegate<TRecord>)dm.CreateDelegate(typeof(LoaderFromReaderByIndexDelegate<TRecord>));
		}

		private void Build(IMetaRecord meta, ref int fieldIndex, LocalBuilder localRecord)
		{
			// keep order in sync with DBORMSelect.PrepareTemplate
			foreach (var field in meta.Fields)
			{
				if (!field.IsFromDB) continue;

				if (field.IsInline)
				{
					BuildForeign(field, ref fieldIndex, localRecord);
				}
				else
				{
					ReadField(field, fieldIndex, localRecord);
					fieldIndex++;
				}
			}

			foreach (var field in meta.Fields)
			{
				if (!field.IsFromDB) continue;
				if (!field.IsEagerLookup) continue;
				BuildForeign(field, ref fieldIndex, localRecord);
			}
		}

		private void BuildForeign(IMetaRecordField field, ref int fieldIndex, LocalBuilder localRecord)
		{
			var type = field.Type;
			var labForeignEnd = il.DefineLabel();
			var labForeignNull = il.DefineLabel();
			var foreignRecord = il.DeclareLocal(type);

			if (field.IsEagerLookup)
			{
				il.Emit(OpCodes.Ldarg_1); // ST: reader
				il.Emit(OpCodes.Ldc_I4, fieldIndex); // ST: reader, fieldIndex
				il.EmitCall(OpCodes.Callvirt, miGetInt64, null); // ST: long
				il.Emit(OpCodes.Brfalse, labForeignNull);
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
			Build(foreignMeta, ref fieldIndex, foreignRecord);

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
