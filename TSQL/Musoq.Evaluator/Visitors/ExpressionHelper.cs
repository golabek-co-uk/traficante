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
        public List<Type> AnonymousTypes = new List<Type>();

        public string GenerateAnonymousTypeName()
        {
            var alpha = "abcdefghijklmnopqrstuwxyz".ToCharArray().Select(x => x.ToString());
            var nextLetter = alpha.Except(AnonymousTypes.Select(x=>x.Name)).First();
            return nextLetter;
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
            AnonymousTypes.Add(dynamicType);
           
            return dynamicType;
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

            dynamicTypeBuilder.DefineMethodOverride(getHashCode, typeof(object).GetMethod("GetHashCode") );
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
                il.Emit(OpCodes.Brfalse_S, goToFalse); // if 0 on the stuck, return false
            }
            il.Emit(OpCodes.Ldc_I4_1); // put true on the stack
            il.Emit(OpCodes.Ret);// return true
            il.MarkLabel(goToFalse);
            il.Emit(OpCodes.Ldc_I4_0); // put false on the stack
            il.Emit(OpCodes.Ret); // return false

            dynamicTypeBuilder.DefineMethodOverride(equals, typeof(object).GetMethod("Equals", new[] { typeof(object) }));
        }
    }

}