using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Musoq.Evaluator.Visitors
{
    public class ExpressionHelper
    {
        private List<Type> _anonymousTypes = new List<Type>();

        public Type CreateAnonymousTypeSameAs(Type type)
        {
            var fields = type.GetFields().Select(x => (x.Name, x.FieldType));
            var newType = CreateAnonymousType(fields);
            return newType;
        }

        public Type CreateAnonymousType(IEnumerable<(string, Type)> fields)
        {
            AssemblyName dynamicAssemblyName = new AssemblyName("Musoq.AnonymousTypes");
            AssemblyBuilder dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(dynamicAssemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder dynamicModule = dynamicAssembly.DefineDynamicModule("Types");

            TypeBuilder dynamicTypeBuilder = dynamicModule.DefineType(GenerateAnonymousTypeName(), TypeAttributes.Public);

            List<FieldBuilder> fieldsBuilder = AddFields(dynamicTypeBuilder, fields);
            OverrideEquals(dynamicTypeBuilder, fieldsBuilder);
            OverrideGetHashCode(dynamicTypeBuilder, fieldsBuilder);

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

        private static List<FieldBuilder> AddFields(TypeBuilder dynamicTypeBuilder, IEnumerable<(string, Type)> fields)
        {
            List<FieldBuilder> fieldsBuilder = new List<FieldBuilder>();
            foreach (var field in fields)
            {
                var fieldBuilder = dynamicTypeBuilder.DefineField(field.Item1, field.Item2, FieldAttributes.Public);
                fieldsBuilder.Add(fieldBuilder);
            }

            return fieldsBuilder;
        }

        private static void OverrideGetHashCode(TypeBuilder dynamicTypeBuilder, List<FieldBuilder> fieldsBuilder)
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

        private static void OverrideEquals(TypeBuilder dynamicTypeBuilder, List<FieldBuilder> fieldsBuilder)
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
                il.Emit(OpCodes.Ceq); //if this.field == obj.field, put 1 else 0 on the stuck
                il.Emit(OpCodes.Brfalse, goToFalse); // if 0 on the stuck, return false
            }
            il.Emit(OpCodes.Ldc_I4_1); // put true on the stack
            il.Emit(OpCodes.Ret);// return true
            il.MarkLabel(goToFalse);
            il.Emit(OpCodes.Ldc_I4_0); // put false on the stack
            il.Emit(OpCodes.Ret); // return false

            dynamicTypeBuilder.DefineMethodOverride(equals, typeof(object).GetMethod("Equals", new[] { typeof(object) }));
        }

        public Type GetQueryableItemType(Expression queryable)
        {
            return queryable.Type.GetGenericArguments()[0]; //IQueryable<AnonymousType>
        }

        public Expression Select(Expression input, Type outputItemType)
        {
            var inputItemType = GetQueryableItemType(input);
            var inputItem = Expression.Parameter(inputItemType, "item_" + inputItemType.Name);

            List<MemberBinding> bindings = new List<MemberBinding>();
            foreach (var field in outputItemType.GetFields())
            {
                //"SelectProp = inputItem.Prop"
                MemberBinding assignment = Expression.Bind(
                    field,
                    Expression.PropertyOrField(inputItem, field.Name));
                bindings.Add(assignment);
            }

            //"new AnonymousType()"
            var creationExpression = Expression.New(outputItemType.GetConstructor(Type.EmptyTypes));

            //"new AnonymousType() { SelectProp = item.name, SelectProp2 = item.SelectProp2) }"
            var initialization = Expression.MemberInit(creationExpression, bindings);

            //"item => new AnonymousType() { SelectProp = item.name, SelectProp2 = item.SelectProp2) }"
            Expression expression = Expression.Lambda(initialization, inputItem);

            var call = Expression.Call(
                typeof(Queryable),
                "Select",
                new Type[] { inputItemType, outputItemType },
                input,
                expression);
            return call;
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
    }
}