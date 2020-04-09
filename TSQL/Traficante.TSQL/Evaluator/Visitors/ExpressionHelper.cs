using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.Json;

namespace Traficante.TSQL.Evaluator.Visitors
{
    public class ExpressionHelper
    {
        private List<Type> _anonymousTypes = new List<Type>();

        private AssemblyName dynamicAssemblyName = null;
        private AssemblyBuilder dynamicAssembly = null;
        private ModuleBuilder dynamicModule = null;

        public ExpressionHelper()
        {
            dynamicAssemblyName = new AssemblyName("AnonymousTypes");
            dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(dynamicAssemblyName, AssemblyBuilderAccess.Run);
            dynamicModule = dynamicAssembly.DefineDynamicModule("Types");
        }

        public Type CreateAnonymousTypeSameAs(Type type, string table = null, string alias = null)
        {
            var fields = type.GetFields().Select(x => (x.Name, x.FieldType));
            var newType = CreateAnonymousType(fields, table, alias);
            return newType;
        }

        public Type CreateWrapperTypeFor(Type type)
        {
            TypeBuilder dynamicTypeBuilder = dynamicModule.DefineType(GenerateAnonymousTypeName(), TypeAttributes.Public);

            var fields = type.GetFields();
            var innerField = dynamicTypeBuilder.DefineField("_inner", type, FieldAttributes.Public);
            foreach (var field in fields)
            {
                PropertyBuilder propertyBuilder = dynamicTypeBuilder.DefineProperty(field.Name, PropertyAttributes.None, field.FieldType, Type.EmptyTypes);

                MethodBuilder getterBuilder = dynamicTypeBuilder.DefineMethod(
                    "get_" + field.Name,
                    MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                    field.FieldType,
                    Type.EmptyTypes);
                ILGenerator getterIL = getterBuilder.GetILGenerator();
                getterIL.Emit(OpCodes.Ldarg_0);
                getterIL.Emit(OpCodes.Ldfld, innerField);
                getterIL.Emit(OpCodes.Ldfld, field);
                getterIL.Emit(OpCodes.Ret);
                propertyBuilder.SetGetMethod(getterBuilder);
            }

            var dynamicType = dynamicTypeBuilder.CreateTypeInfo();
            _anonymousTypes.Add(dynamicType);

            return dynamicType;
        }

        public Type CreateAnonymousType(IEnumerable<(string Name, Type Type)> fields, string tableName = null, string tableAlias = null)
        {
            TypeBuilder dynamicTypeBuilder = dynamicModule.DefineType(GenerateAnonymousTypeName(), TypeAttributes.Public);

            List<FieldBuilder> fieldsBuilder = AddFields(dynamicTypeBuilder, fields);
            List<FieldInfo> fieldsInfo = fieldsBuilder.Select(x => (FieldInfo)x).ToList();
            OverrideEquals(dynamicTypeBuilder, fieldsInfo);
            OverrideGetHashCode(dynamicTypeBuilder, fieldsInfo);
            if (tableName != null || tableAlias != null)
                AddTableAttribute(dynamicTypeBuilder, tableName, tableAlias);

            var dynamicType = dynamicTypeBuilder.CreateTypeInfo();
            _anonymousTypes.Add(dynamicType);

            return dynamicType;
        }

        private string GenerateAnonymousTypeName()
        {
            var alpha = "abcdefghijklmnopqrstuwxyz".ToCharArray().Select(x => x.ToString());
            var nextLetter = alpha.Except(_anonymousTypes.Select(x => x.Name)).First();
            return nextLetter;
        }

        private static List<FieldBuilder> AddFields(TypeBuilder dynamicTypeBuilder, IEnumerable<(string Name, Type Type)> fields)
        {
            List<FieldBuilder> fieldsBuilder = new List<FieldBuilder>();
            foreach (var field in fields)
            {
                var fieldBuilder = dynamicTypeBuilder.DefineField(field.Name, field.Type, FieldAttributes.Public);
                fieldsBuilder.Add(fieldBuilder);
            }

            return fieldsBuilder;
        }

        private static void OverrideGetHashCode(TypeBuilder dynamicTypeBuilder, List<FieldInfo> fieldsBuilder)
        {
            //Pick two different prime numbers, e.g. 17 and 23, and do:
            //int hash = 17;
            //hash = hash * 23 + field1.GetHashCode();
            //hash = hash * 23 + field2.GetHashCode();
            //hash = hash * 23 + field3.GetHashCode();
            //return hash;

            MethodBuilder getHashCode = dynamicTypeBuilder.DefineMethod(
                "GetHashCode",
                MethodAttributes.Public
                | MethodAttributes.HideBySig
                | MethodAttributes.NewSlot
                | MethodAttributes.Virtual
                | MethodAttributes.Final,
                CallingConventions.HasThis,
                typeof(int),
                new Type[] { });

            var il = getHashCode.GetILGenerator();
            il.Emit(OpCodes.Ldc_I4_S, 17); // put "17" on the stack
            foreach (var field in fieldsBuilder)
            {
                if (field.FieldType.GenericTypeArguments.Length > 0)
                    continue;
                
                Label gotoIsNull = il.DefineLabel();
                
                il.Emit(OpCodes.Ldarg_0); // put "this" on the stack
                il.Emit(OpCodes.Ldfld, field); // put "this.field" on the stack
                il.Emit(OpCodes.Ldnull); // put "null" on the stack
                il.Emit(OpCodes.Ceq); // if "this.field" is "null"
                il.Emit(OpCodes.Brtrue, gotoIsNull); // if "this.field" is null, goto IsNull

                // "this.field" is not null
                il.Emit(OpCodes.Ldc_I4_S, 23); // put "23" on the stack
                il.Emit(OpCodes.Mul); // multiply "23 x last hash" and put result on the stack

                if (field.FieldType.IsValueType)
                {

                    int localIndex = il.DeclareLocal(field.FieldType).LocalIndex; // declare the local variable
                    il.Emit(OpCodes.Ldarg_0); // put "this" on the stack
                    il.Emit(OpCodes.Ldfld, field); // put "this.field" on the stack
                    il.Emit(OpCodes.Stloc, localIndex); // assign the value to the local variable
                    il.Emit(OpCodes.Ldloca_S, localIndex); // load reference to the value from the local variable
                }
                else
                {
                    il.Emit(OpCodes.Ldarg_0); // put "this" on the stack
                    il.Emit(OpCodes.Ldfld, field); // put "this.field" on the stack
                }
                 
                il.Emit(OpCodes.Call, field.FieldType.GetMethod("GetHashCode", new Type[] { })); // call "GetHashCode" and put result on the stack
                il.Emit(OpCodes.Add); // add result of "23 x last hash"  to result of "GetHashCode" and put is on the stack

                // "this.field" is null, do nothing
                il.MarkLabel(gotoIsNull); // IsNull label
            }

            il.Emit(OpCodes.Ret); // return number

            dynamicTypeBuilder.DefineMethodOverride(getHashCode, typeof(object).GetMethod("GetHashCode"));
        }

        private static void OverrideEquals(TypeBuilder dynamicTypeBuilder, List<FieldInfo> fieldsBuilder)
        {
            MethodBuilder equals = dynamicTypeBuilder.DefineMethod(
                            "Equals",
                            MethodAttributes.Public
                            | MethodAttributes.HideBySig
                            | MethodAttributes.NewSlot
                            | MethodAttributes.Virtual
                            | MethodAttributes.Final,
                            CallingConventions.HasThis,
                            typeof(bool),
                            new Type[] { typeof(object) });

            var il = equals.GetILGenerator();

            Label goToFalse = il.DefineLabel();

            foreach (var field in fieldsBuilder)
            {
                il.Emit(OpCodes.Ldarg_0); // put "this" on the stack
                il.Emit(OpCodes.Ldfld, field); // put "this.field" on the stack 
                il.Emit(OpCodes.Ldarg_1); //put "objecToCompare" on the stack
                il.Emit(OpCodes.Ldfld, field); //put "objecToCompare.field" on the stack
                il.Emit(OpCodes.Call, typeof(Object).GetMethod("Equals", new Type[] { typeof(object), typeof(object) }));
                il.Emit(OpCodes.Brfalse, goToFalse); //if Equals returned false, go to  "goToFalse" lable
            }
            il.Emit(OpCodes.Ldc_I4_1); // put true on the stack
            il.Emit(OpCodes.Ret);// return true
            il.MarkLabel(goToFalse);
            il.Emit(OpCodes.Ldc_I4_0); // put false on the stack
            il.Emit(OpCodes.Ret); // return false
            
            dynamicTypeBuilder.DefineMethodOverride(equals, typeof(object).GetMethod("Equals", new[] { typeof(object) }));
        }

        public Type CreateEqualityComparerForType(Type objType, string[] propsToCompare)
        {
            var comparerInterface = typeof(IEqualityComparer<>).MakeGenericType(objType);
            TypeBuilder dynamicTypeBuilder = dynamicModule.DefineType(objType.Name + "_EqualityComparer", TypeAttributes.Public, typeof(object), new[] { comparerInterface });
            var fieldsToCompare = objType.GetFields().Where(x => propsToCompare.Contains(x.Name)).ToList();
            AddEqualsMethod(dynamicTypeBuilder, objType, fieldsToCompare);
            AddGetHashCodeMethod(dynamicTypeBuilder, objType, fieldsToCompare);
            var qualityComparerType = dynamicTypeBuilder.CreateTypeInfo();
            return qualityComparerType;
        }

        private static void AddEqualsMethod(TypeBuilder dynamicTypeBuilder, Type objType, List<FieldInfo> fieldsBuilder)
        {
            MethodBuilder equals = dynamicTypeBuilder.DefineMethod(
                            "Equals",
                            MethodAttributes.Public
                            | MethodAttributes.HideBySig
                            | MethodAttributes.NewSlot
                            | MethodAttributes.Virtual
                            | MethodAttributes.Final,
                            CallingConventions.HasThis,
                            typeof(bool),
                            new Type[] { objType, objType });

            var il = equals.GetILGenerator();
            Label goToFalse = il.DefineLabel();

            foreach (var field in fieldsBuilder)
            {
                il.Emit(OpCodes.Ldarg_1); // put "obj1" on the stack
                il.Emit(OpCodes.Ldfld, field); // put "obj1.field" on the stack 
                il.Emit(OpCodes.Ldarg_2); //put "obj2" on the stack
                il.Emit(OpCodes.Ldfld, field); //put "obj2.field" on the stack

                il.Emit(OpCodes.Ceq); //if obj1.field == obj2.field, put 1 else 0 on the stuck
                il.Emit(OpCodes.Brfalse, goToFalse); // if 0 on the stuck, return false
                // TODO: replace OpCodes.Ceq with the following code
                //il.Emit(OpCodes.Call, typeof(Object).GetMethod("Equals", new Type[] { typeof(object), typeof(object) }));
                //il.Emit(OpCodes.Brfalse, goToFalse); //if Equals returned false, go to  "goToFalse" lable
            }
            il.Emit(OpCodes.Ldc_I4_1); // put true on the stack
            il.Emit(OpCodes.Ret);// return true
            il.MarkLabel(goToFalse);
            il.Emit(OpCodes.Ldc_I4_0); // put false on the stack
            il.Emit(OpCodes.Ret); // return false
        }

        private static void AddGetHashCodeMethod(TypeBuilder dynamicTypeBuilder, Type objType, List<FieldInfo> fieldsBuilder)
        {
            //Pick two different prime numbers, e.g. 17 and 23, and do:
            //int hash = 17;
            //hash = hash * 23 + field1.GetHashCode();
            //hash = hash * 23 + field2.GetHashCode();
            //hash = hash * 23 + field3.GetHashCode();
            //return hash;

            MethodBuilder getHashCode = dynamicTypeBuilder.DefineMethod(
                "GetHashCode",
                MethodAttributes.Public
                | MethodAttributes.HideBySig
                | MethodAttributes.NewSlot
                | MethodAttributes.Virtual
                | MethodAttributes.Final,
                CallingConventions.HasThis,
                typeof(int),
                new Type[] { objType });

            var il = getHashCode.GetILGenerator();
            il.Emit(OpCodes.Ldc_I4_S, 17); // put "17" on the stack
            foreach (var field in fieldsBuilder)
            {
                Label gotoIsNull = il.DefineLabel();

                il.Emit(OpCodes.Ldarg_1); // put "obj" on the stack
                il.Emit(OpCodes.Ldfld, field); // put "obj.field" on the stack
                il.Emit(OpCodes.Ldnull); // put "null" on the stack
                il.Emit(OpCodes.Ceq); // if "obj.field" is "null"
                il.Emit(OpCodes.Brtrue, gotoIsNull); // if "this.field" is null, goto IsNull

                // "this.field" is not null
                il.Emit(OpCodes.Ldc_I4_S, 23); // put "23" on the stack
                il.Emit(OpCodes.Mul); // multiply "23 x last hash" and put result on the stack

                if (field.FieldType.IsValueType)
                {

                    int localIndex = il.DeclareLocal(field.FieldType).LocalIndex; // declare the local variable
                    il.Emit(OpCodes.Ldarg_1); // put "obj" on the stack
                    il.Emit(OpCodes.Ldfld, field); // put "obj.field" on the stack
                    il.Emit(OpCodes.Stloc, localIndex); // assign the value to the local variable
                    il.Emit(OpCodes.Ldloca_S, localIndex); // load reference to the value from the local variable
                }
                else
                {
                    il.Emit(OpCodes.Ldarg_1); // put "obj" on the stack
                    il.Emit(OpCodes.Ldfld, field); // put "obj.field" on the stack
                }
                il.Emit(OpCodes.Call, field.FieldType.GetMethod("GetHashCode", new Type[] { })); // call "GetHashCode" and put result on the stack
                il.Emit(OpCodes.Add); // add result of "23 x last hash"  to result of "GetHashCode" and put is on the stack

                // "this.field" is null, do nothing
                il.MarkLabel(gotoIsNull); // IsNull label
            }

            il.Emit(OpCodes.Ret); // return number
        }

        private void AddTableAttribute(TypeBuilder dynamicTypeBuilder, string name, string alias)
        {
            var aliasAttributeConctructor = typeof(TableAttribute).GetConstructor(new Type[] { typeof(string), typeof(string) });
            var aliasAttributeBuilder = new CustomAttributeBuilder(aliasAttributeConctructor, new object[] { name, alias });
            dynamicTypeBuilder.SetCustomAttribute(aliasAttributeBuilder);
        }
        
        public Type GetItemType(Expression queryable)
        {
            return queryable.Type.GetGenericArguments()[0]; //IQueryable<AnonymousType>
        }

        public  (Expression, Expression) AlignSimpleTypes(Expression left, Expression right)
        {
            if (left.Type.IsValueType == false || right.Type.IsValueType == false)
            {
                return (left, right);
            }
            if (left.Type.Name == "Nullable`1" ^ right.Type.Name == "Nullable`1")
            {
               if ( left.Type.Name != "Nullable`1")
                    left = Expression.Convert(left, typeof(Nullable<>).MakeGenericType(left.Type));
                if (right.Type.Name != "Nullable`1")
                    right = Expression.Convert(right, typeof(Nullable<>).MakeGenericType(right.Type));
            }

            if (left.Type == typeof(Nullable<JsonElement>) || right.Type == typeof(Nullable<JsonElement>))
            {
                left = Expression.Call(
                     Expression.Convert(left, typeof(object)),
                     typeof(object).GetMethod("ToString"));
                right = Expression.Call(
                     Expression.Convert(right, typeof(object)),
                     typeof(object).GetMethod("ToString"));

                //right = Expression.Convert(right, typeof(string));
                //left = Expression.Convert(left, typeof(string));
                return (left, right);
            }

            //TODO: check best converstoin
            //System.Byte
            //System.SByte
            //System.Int16
            //System.UInt16
            //System.Int32
            //System.UInt32
            //System.Int64
            //System.UInt64
            //System.Single
            //System.Double
            //System.Decimal
            if (left.Type == typeof(System.Decimal) ^ right.Type == typeof(System.Decimal))
            {
                if (left.Type == typeof(System.Decimal))
                    right = Expression.Convert(right, left.Type);
                else
                    left = Expression.Convert(left, right.Type);
            }
            if (left.Type == typeof(System.Int64) ^ right.Type == typeof(System.Int64))
            {
                if (left.Type == typeof(System.Int64))
                    right = Expression.Convert(right, left.Type);
                else
                    left = Expression.Convert(left, right.Type);
            }

            return (left, right);
        }

        public Expression AlignSimpleTypes(Expression left, Type right)
        {
            //if (right.Name == "Nullable`1")
            //{
            //    if (left.Type.IsValueType && left.Type.Name != "Nullable`1")
            //    {
            //        return Expression.Convert(left, typeof(Nullable<>).MakeGenericType(left.Type));
            //    }
            //}
            if (left.Type != right && right.IsArray == false) // right.IsArray == false -> because of "params" argument
            {
                if (right == typeof(string))
                {
                    var toString = typeof(Object).GetMethod("ToString");
                    return Expression.Call(left, toString);
                }
                return Expression.Convert(left, right);
            }
            return left;
        }

        public Expression SqlLikeOperation(Expression left, Expression right, Func<Expression, Expression,Expression> operation)
        {
            (left, right) = this.AlignSimpleTypes(left, right);

            if (left.Type.IsValueType && right.Type.IsValueType && left.Type.Name != "Nullable`1")
            {

                return operation(left, right);
            }
            else
            {
                var leftIsNotNull = Expression.NotEqual(left, Expression.Default(left.Type));
                var rightIsNotNull = Expression.NotEqual(right, Expression.Default(right.Type));
                var bothAreNotNull = Expression.And(leftIsNotNull, rightIsNotNull);
                return  Expression.And(bothAreNotNull, operation(left, right));
            }
        }

        
        public Expression PropertyOrField(Expression obj, string fieldName, Type fieldType)
        {
            return
                Expression.Condition(
                    Expression.Equal(obj, Expression.Default(obj.Type)),
                    Expression.Default(fieldType),
                    Expression.PropertyOrField(obj, fieldName));
        }
    }

    public class TableAttribute : Attribute
    {
        public string Name { get; set; }
        public string Alias { get; set; }

        public TableAttribute(string table, string alias)
        {
            Name = table;
            Alias = alias;
        }
    }
}