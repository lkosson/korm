using Kosson.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Kosson.KRUD.RecordLoader
{
	class LoaderBuilder
	{
		private IMetaBuilder metabuilder;
		private MethodInfo miGetItemByName;
		private MethodInfo miGetItemByIndex;
		private MethodInfo miConvert;
		private MethodInfo miCreate;
		private MethodInfo miGetType;
		private List<IMetaRecordField[]> fieldMapping;

		public LoaderBuilder()
		{
			metabuilder = KORMContext.Current.MetaBuilder;
			miGetItemByName = typeof(IRow).GetMethod("get_Item", new[] { typeof(string) });
			miGetItemByIndex = typeof(IIndexBasedRow).GetMethod("get_Item", new[] { typeof(int) });
			miConvert = typeof(IConverter).GetMethod("Convert", new[] { typeof(object), typeof(Type) });
			miCreate = typeof(FactoryExtensions).GetMethod("Create", new[] { typeof(IFactory), typeof(Type) });
			miGetType = typeof(Type).GetMethod("GetTypeFromHandle");
		}

		public LoaderByNameDelegate<T> BuildByName<T>()
		{
			var dm = new DynamicMethod("Load", null, new[] { typeof(T), typeof(IRow), typeof(IConverter), typeof(IFactory) }, true);
			var il = dm.GetILGenerator();

			var local = il.DeclareLocal(typeof(T));
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Stloc, local);

			Build(il, typeof(T), "", new Stack<IMetaRecordField>(8), local);

			il.Emit(OpCodes.Ret);
			return (LoaderByNameDelegate<T>)dm.CreateDelegate(typeof(LoaderByNameDelegate<T>));
		}

		public LoaderByIndexDelegate<T> BuildByIndex<T>(List<IMetaRecordField[]> fieldMapping)
		{
			this.fieldMapping = fieldMapping;

			var dm = new DynamicMethod("Load", null, new[] { typeof(T), typeof(IIndexBasedRow), typeof(IConverter), typeof(IFactory) }, true);
			var il = dm.GetILGenerator();

			var local = il.DeclareLocal(typeof(T));
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Stloc, local);

			Build(il, typeof(T), "", new Stack<IMetaRecordField>(8), local);

			il.Emit(OpCodes.Ret);
			return (LoaderByIndexDelegate<T>)dm.CreateDelegate(typeof(LoaderByIndexDelegate<T>));
		}

		private void Build(ILGenerator il, Type type, string prefix, Stack<IMetaRecordField> path, LocalBuilder local)
		{
			var meta = metabuilder.Get(type);

			var filter = meta.IsTable || meta.Fields.Where(f => f.IsFromDB).Any();

			foreach (var field in meta.Fields)
			{
				if (filter && !field.IsFromDB) continue;
				Build(il, field, prefix, path, local);
			}
		}

		private void Build(ILGenerator il, IMetaRecordField field, string prefix, Stack<IMetaRecordField> path, LocalBuilder local)
		{
			path.Push(field);
			if (field.IsEagerLookup || field.IsInline)
				BuildForeignLoader(il, field, prefix, path, local);
			else
				BuildLocalLoader(il, field, prefix, path, local);
			path.Pop();
		}

		private void BuildForeignLoader(ILGenerator il, IMetaRecordField field, string prefix, Stack<IMetaRecordField> path, LocalBuilder local)
		{
			var type = field.Type;
			var miGet = field.Property.GetGetMethod();
			var miSet = field.Property.GetSetMethod();

			var labForeignEnd = il.DefineLabel();
			var labForeignConstr = il.DefineLabel();
			var labForeignSet = il.DefineLabel();
			var labForeignNull = il.DefineLabel();
			var locForeign = il.DeclareLocal(type);

			if (field.IsEagerLookup)
			{
				il.Emit(OpCodes.Ldarg_1); // ST: Row
				if (fieldMapping == null)
				{
					il.Emit(OpCodes.Ldstr, prefix + field.Name); // ST: Row, FieldName
					il.EmitCall(OpCodes.Callvirt, miGetItemByName, null); // ST: Value
				}
				else
				{
					int idx = fieldMapping.Count;
					il.Emit(OpCodes.Ldc_I4, idx); // ST: Row, FieldIndex
					il.EmitCall(OpCodes.Callvirt, miGetItemByIndex, null); // ST: Value
					fieldMapping.Add(path.Reverse().ToArray());
				}
				il.Emit(OpCodes.Brfalse, labForeignNull);
			}

			// _tmp = local.(field.Name);
			il.Emit(OpCodes.Ldloc, local); // ST: Record
			il.EmitCall(OpCodes.Callvirt, miGet, null); // ST: foreign-or-null
			il.Emit(OpCodes.Stloc, locForeign); // ST: <0>

			// if (_tmp != null) goto end;
			il.Emit(OpCodes.Ldloc, locForeign); // ST: foreign-or-null
			il.Emit(OpCodes.Brtrue_S, labForeignEnd); // ST: <0>

			// if (factory == null) goto constr;
			il.Emit(OpCodes.Ldarg_3); // ST: Factory
			il.Emit(OpCodes.Brfalse_S, labForeignConstr); // ST: <0>

			// _tmp = factory.Create<T>();
			il.Emit(OpCodes.Ldarg_3); // ST: Factory
			il.Emit(OpCodes.Ldtoken, type); // ST: Factory, <Type>
			il.EmitCall(OpCodes.Call, miGetType, null); // ST: Factory, Type
			il.EmitCall(OpCodes.Call, miCreate, null); // ST: foreign-as-obj
			if (type.GetTypeInfo().IsValueType)
				il.Emit(OpCodes.Unbox_Any, type); // ST: foreign
			else
				il.Emit(OpCodes.Castclass, type); // ST: foreign
			il.Emit(OpCodes.Stloc, locForeign); // ST: <0>

			// goto set
			il.Emit(OpCodes.Br, labForeignSet); // ST: <0>

			// constr:
			il.MarkLabel(labForeignConstr); // ST: <0>

			// _tmp = new <Type>();
			il.Emit(OpCodes.Newobj, type.GetConstructor(Type.EmptyTypes)); // ST: foreign
			il.Emit(OpCodes.Stloc, locForeign); // ST: <0>

			// set:
			il.MarkLabel(labForeignSet); // ST: <0>

			// local.(field.Name) = _tmp
			il.Emit(OpCodes.Ldloc, local); // ST: Record
			il.Emit(OpCodes.Ldloc, locForeign); // ST: Record, foreign
			il.EmitCall(OpCodes.Callvirt, miSet, null); // ST: <0>

			// end:
			il.MarkLabel(labForeignEnd); // ST: <0>

			Build(il, field.Type, prefix + field.Name + ".", path, locForeign);

			// null:
			il.MarkLabel(labForeignNull); // ST: <0>
		}

		private void BuildLocalLoader(ILGenerator il, IMetaRecordField field, string prefix, Stack<IMetaRecordField> path, LocalBuilder localRecord)
		{
			var miSet = field.Property.GetSetMethod();
			var type = field.Type;

			var lblNotNull = il.DefineLabel();
			var lblNeedConvert = il.DefineLabel();
			var lblEnd = il.DefineLabel();
			var localField = il.DeclareLocal(typeof(Object));
			var localFieldTypedValue = type.IsValueType ? il.DeclareLocal(type) : null;

			// localField = row[prefix+field.Name]
			// ST: <0>
			il.Emit(OpCodes.Ldarg_1); // ST: Row
			if (fieldMapping == null)
			{
				il.Emit(OpCodes.Ldstr, prefix + field.Name); // ST: Row, FieldName
				il.EmitCall(OpCodes.Callvirt, miGetItemByName, null); // ST: Value-as-object
			}
			else
			{
				int idx = fieldMapping.Count;
				il.Emit(OpCodes.Ldc_I4, idx); // ST: Row, FieldIndex
				il.EmitCall(OpCodes.Callvirt, miGetItemByIndex, null); // ST: Value-as-object
				fieldMapping.Add(path.Reverse().ToArray());
			}
			il.Emit(OpCodes.Stloc, localField); // ST: <0>

			// if (localField == null) localRecord.(field.Name) = null;
			// ST: <0>
			il.Emit(OpCodes.Ldloc, localField); // ST: Value-as-object
			il.Emit(OpCodes.Brtrue, lblNotNull); // ST: <0>
			il.Emit(OpCodes.Ldloc, localRecord); // ST: Record
			if (type.IsValueType)
			{
				il.Emit(OpCodes.Ldloca, localFieldTypedValue);
				il.Emit(OpCodes.Initobj, type);
				il.Emit(OpCodes.Ldloc, localFieldTypedValue);
			}
			else
			{
				il.Emit(OpCodes.Ldnull); // ST: Record, null
			}
			il.EmitCall(OpCodes.Callvirt, miSet, null); // ST: <0>
			il.Emit(OpCodes.Br, lblEnd); // ST: <0>

			il.MarkLabel(lblNotNull); // ST: <0>
			// if (localField is type) localRecord.(field.Name) = localField;
			il.Emit(OpCodes.Ldloc, localRecord); // ST: Record
			if (typeof(IRecordRef).IsAssignableFrom(type))
			{
				var lblNotRecordRef = il.DefineLabel();
				il.Emit(OpCodes.Ldloc, localField); // ST: Record, Value-as-object
				il.Emit(OpCodes.Isinst, typeof(long)); // ST: Record, Value-or-null
				il.Emit(OpCodes.Dup); // ST: Record, Value-or-null, Value-or-null
				il.Emit(OpCodes.Brfalse, lblNotRecordRef); // ST: Record, Value-or-null
				il.Emit(OpCodes.Unbox_Any, typeof(long)); // ST: Record, Value-as-long
				il.Emit(OpCodes.Newobj, type.GetConstructor(new[] { typeof(long) })); // ST: Record, RecordRef
				il.EmitCall(OpCodes.Callvirt, miSet, null); // ST: <0>
				il.Emit(OpCodes.Br, lblEnd); // ST: <0>

				il.MarkLabel(lblNotRecordRef); // ST: Record, Value-or-null
				il.Emit(OpCodes.Pop); // ST: Record
			}

			il.Emit(OpCodes.Ldloc, localField); // ST: Record, Value-as-object
			il.Emit(OpCodes.Isinst, type); // ST: Record, Value-or-null
			il.Emit(OpCodes.Dup); // ST: Record, Value-or-null, Value-or-null
			il.Emit(OpCodes.Brfalse, lblNeedConvert); // ST: Record, Value-or-null

			if (type.IsValueType)
				il.Emit(OpCodes.Unbox_Any, type); // ST: Record, CastedValue
			else
				il.Emit(OpCodes.Castclass, type); // ST: Record, CastedValue
			il.EmitCall(OpCodes.Callvirt, miSet, null); // ST: <0>
			il.Emit(OpCodes.Br, lblEnd); // ST: <0>

			il.MarkLabel(lblNeedConvert); // ST: Record, Value-or-null
			// local.(field.Name) = converter.Convert(localField);
			il.Emit(OpCodes.Pop); // ST: Record
			il.Emit(OpCodes.Ldarg_2); // ST: Record, Converter
			il.Emit(OpCodes.Ldloc, localField); // ST: Record, Converter, Value-as-object

			if (typeof(IRecordRef).IsAssignableFrom(type))
			{
				il.Emit(OpCodes.Ldtoken, typeof(long)); // ST: Record, Converter, Value-as-object, <long>
				il.EmitCall(OpCodes.Call, miGetType, null); // ST: Record, Converter, Value-as-object, Type
				il.EmitCall(OpCodes.Callvirt, miConvert, null); // ST: Record, BoxedLongValue
				il.Emit(OpCodes.Unbox_Any, typeof(long)); // ST: Record, LongValue
				il.Emit(OpCodes.Newobj, type.GetConstructor(new[] { typeof(long) })); // ST: Record, RecordRef
			}
			else
			{
				il.Emit(OpCodes.Ldtoken, type); // ST: Record, Converter, Value-as-object, <Type>
				il.EmitCall(OpCodes.Call, miGetType, null); // ST: Record, Converter, Value-as-object, Type
				il.EmitCall(OpCodes.Callvirt, miConvert, null); // ST: Record, CastedValue
				if (type.GetTypeInfo().IsValueType)
					il.Emit(OpCodes.Unbox_Any, type); // ST: Record, CastedValue
				else
					il.Emit(OpCodes.Castclass, type); // ST: Record, CastedValue
			}
			il.EmitCall(OpCodes.Callvirt, miSet, null); // ST: <0>

			il.MarkLabel(lblEnd); // ST: <0>
		}
	}
}
