using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace Traficante.TSQL.Evaluator.Helpers
{
    static public class ExpressionHelpers
    {

        static public Type GetElementType(this Expression sequence)
        {
            return sequence.Type.GenericTypeArguments.Length > 0 ?
                sequence.Type.GetGenericArguments()[0] :
                sequence.Type.GetElementType();
        }

        static public bool HasTableAttribute(this Type type)
        {
            var aliasAttribute = (TableAttribute)type.GetCustomAttributes(typeof(TableAttribute), false).FirstOrDefault();
            return aliasAttribute != null;
        }

        static public TableAttribute GetTableAttribute(this Type type)
        {
            return (TableAttribute)type.GetCustomAttributes(typeof(TableAttribute), false).FirstOrDefault();
        }

        static public FieldAttribute GetFieldAttribute(this FieldInfo fieldInfo)
        {
            return (FieldAttribute)fieldInfo.GetCustomAttributes(typeof(FieldAttribute), false).FirstOrDefault();
        }

        static public bool IsGrouping(this Type type)
        {
            return type.Name == "IGrouping`2";
        }

        static public bool IsSequence(this Type type)
        {
            return type.Name == "ParallelQuery`1";
        }

        static public Type GetGropingKeyType(this Type type)
        {
            return type.GetGenericArguments()[0];
        }

        static public Type GetGroupingElementType(this Type type)
        {
            return type.GetGenericArguments()[1];
        }

        //static public Expression AccessAlias(this Expression parameter, string alias)
        //{
        //    if (parameter.Type.Name == "IGrouping`2")
        //    {
        //        parameter = parameter.PropertyOrField("Key");
        //    }

        //    var type = parameter.Type;
        //    var tableAttribute = type.GetTableAttribute();
        //    if (tableAttribute != null)
        //    {
        //        if (string.Equals(tableAttribute.Alias, alias, StringComparison.InvariantCultureIgnoreCase) ||
        //            string.Equals(tableAttribute.Name, alias, StringComparison.InvariantCultureIgnoreCase))
        //        {
        //            return parameter;
        //        }
        //    }

        //    foreach (var field in type.GetFields())
        //    {
        //        tableAttribute = field.FieldType.GetTableAttribute();
        //        if (tableAttribute != null)
        //        {
        //            if (string.Equals(tableAttribute.Alias, alias, StringComparison.InvariantCultureIgnoreCase) ||
        //                string.Equals(tableAttribute.Name, alias, StringComparison.InvariantCultureIgnoreCase))
        //            {
        //                return parameter.PropertyOrField(field.Name);
        //            }
        //        }
        //    }

        //    throw new Exception($"Cannot find alias: {alias}");
        //}

        static public List<(string Name, Type Type, string ColumnName, string TableName, string TableAlias, Expression Expression)> GetAllFields(this Expression parameter)
        {
            List<(string Name, Type Type, string ColumnName, string Table, string Alias, Expression Expression)> fields = new List<(string Name, Type Type, string ColumnName, string Table, string Alias, Expression Expression)>();
            
            if (parameter.Type.Name == "IGrouping`2")
            {
                parameter = parameter.PropertyOrField("Key");
            }

            var type = parameter.Type;
            if (type.Namespace.StartsWith("System"))
                return fields;

            var tableAttribute = type.GetTableAttribute();
            if (tableAttribute != null)
            {
                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    var fieldAttribute = field.GetFieldAttribute();
                    if (fieldAttribute != null)
                        fields.Add((field.Name, field.FieldType, fieldAttribute.ColumnName, fieldAttribute.TableName, fieldAttribute.TableAlias, parameter.PropertyOrField(field.Name)));
                    else
                        fields.Add((field.Name, field.FieldType, field.Name, tableAttribute.Name, tableAttribute.Alias, parameter.PropertyOrField(field.Name)));
                }
            }
            else
            {
                foreach (var innerField in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    tableAttribute = innerField.FieldType.GetTableAttribute();
                    if (tableAttribute != null)
                    {
                        foreach (var field in innerField.FieldType.GetFields())
                        {
                            var fieldAttribute = field.GetFieldAttribute();
                            if (fieldAttribute != null)
                                fields.Add((field.Name, field.FieldType, fieldAttribute.ColumnName, fieldAttribute.TableName, fieldAttribute.TableAlias, parameter.PropertyOrField(innerField.Name).PropertyOrField(field.Name)));
                            else
                                fields.Add((field.Name, field.FieldType, field.Name, tableAttribute.Name, tableAttribute.Alias, parameter.PropertyOrField(innerField.Name).PropertyOrField(field.Name)));
                        }
                    }
                }
            }
            return fields;
        }

        static public List<(string Name, Type Type, Expression Expression)> GetInnerFields(this Expression parameter)
        {
            List<(string Name, Type Type, Expression Expression)> fields = new List<(string Name, Type Type, Expression Expression)>();

            var type = parameter.Type;
            if (type.Namespace.StartsWith("System"))
                return fields;

            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                fields.Add((field.Name, field.FieldType,  parameter.PropertyOrField(field.Name)));
            }
            foreach (var property in type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Except(type.GetDefaultMembers().OfType<PropertyInfo>()))
            {
                fields.Add((property.Name, property.PropertyType, parameter.PropertyOrField(property.Name)));
            }

            return fields;
        }


        //static public List<(string Name, Type Type, string ColumnName, string TableName, string TableAlias, Expression Expression)> GetFields(this Expression parameter, string fieldName)
        //{
        //    var allFields = parameter.GetAllFields();
        //    return allFields.Where(x => 
        //            string.Equals(x.Name, fieldName, StringComparison.InvariantCultureIgnoreCase) ||
        //            string.Equals(x.ColumnName, fieldName, StringComparison.InvariantCultureIgnoreCase))
        //        .ToList();
        //}

        //static public (string Name, Type Type, string ColumnName, string TableName, string TableAlias, Expression Expression) GetField(this Expression parameter, string fieldName, string alias)
        //{
        //    var allFields = parameter.GetAllFields();
        //    return allFields.Where(x =>
        //        (string.Equals(x.Name, fieldName, StringComparison.InvariantCultureIgnoreCase) ||
        //         string.Equals(x.ColumnName, fieldName, StringComparison.InvariantCultureIgnoreCase)) 
        //        &&
        //        (string.Equals(x.TableAlias, alias, StringComparison.InvariantCultureIgnoreCase) ||
        //         string.Equals(x.TableName, alias, StringComparison.InvariantCultureIgnoreCase)))
        //        .FirstOrDefault();
        //}

        static public List<(string Name, Type Type, string ColumnName, string TableName, string TableAlias, Expression Expression)> GetFields(this Expression parameter, string[] path)
        {
            if (path.Length == 0)
                return default;

            if (path.Length == 1)
            {
                var allFields = parameter.GetAllFields();
                return allFields.Where(x =>
                     (string.Equals(x.Name, path[0], StringComparison.InvariantCultureIgnoreCase) ||
                      string.Equals(x.ColumnName, path[0], StringComparison.InvariantCultureIgnoreCase))
                    ||
                     (string.Equals(x.TableAlias, path[0], StringComparison.InvariantCultureIgnoreCase) ||
                      string.Equals(x.TableName, path[0], StringComparison.InvariantCultureIgnoreCase)))
                    .ToList();
            }
            else
            {
                var allFields = parameter.GetAllFields();
                var startingWithAlias =
                    allFields.Where(x =>
                        (string.Equals(x.TableAlias, path[0], StringComparison.InvariantCultureIgnoreCase) ||
                        string.Equals(x.TableName, path[0], StringComparison.InvariantCultureIgnoreCase))
                        &&
                        (string.Equals(x.Name, path[1], StringComparison.InvariantCultureIgnoreCase) ||
                        string.Equals(x.ColumnName, path[1], StringComparison.InvariantCultureIgnoreCase)))
                    .SelectMany(x => path.Length > 2 ?
                        x.Expression.GetInnerFields(path.Skip(2).ToArray()).Select(y => (y.Name, y.Type, x.ColumnName, x.TableName, x.TableAlias, y.Expression)).ToList() :
                        new List<(string Name, Type Type, string ColumnName, string TableName, string TableAlias, Expression Expression)> { x })
                .ToList();

                var startingWithField = allFields.Where(x =>
                        (string.Equals(x.Name, path[0], StringComparison.InvariantCultureIgnoreCase) ||
                        string.Equals(x.ColumnName, path[0], StringComparison.InvariantCultureIgnoreCase)))
                    .SelectMany(x => path.Length > 1 ?
                        x.Expression.GetInnerFields(path.Skip(1).ToArray()).Select(y => (y.Name, y.Type, x.ColumnName, x.TableName, x.TableAlias, y.Expression)).ToList() :
                        new List<(string Name, Type Type, string ColumnName, string TableName, string TableAlias, Expression Expression)> { x })
                .ToList();

                return startingWithAlias.Union(startingWithField).ToList();
            }
        }

        static public List<(string Name, Type Type, Expression Expression)> GetInnerFields(this Expression parameter, string[] path)
        {
            if (path.Length == 0)
                return default;

            return parameter
                .GetInnerFields()
                .Where(x => string.Equals(x.Name, path[0], StringComparison.InvariantCultureIgnoreCase))
                .SelectMany(x => path.Length > 1 ?
                    x.Expression.GetInnerFields(path.Skip(1).ToArray()) :
                    new List<(string Name, Type Type, Expression Expression)> { x })
                .ToList();
        }

        static public Expression PropertyOrField(this Expression expression, string propertyOrField)
        {
            var tableAttribute = expression.Type.GetTableAttribute();
            var field = expression.Type.GetFields().FirstOrDefault(x => string.Equals(x.Name, propertyOrField, StringComparison.InvariantCultureIgnoreCase));
            if (field != null)
            {
                var e =  Expression.Condition(
                    Expression.Equal(expression, Expression.Default(expression.Type)),
                    Expression.Default(field.FieldType),
                    Expression.PropertyOrField(expression, field.Name));
                return new FieldExpression(e, propertyOrField, tableAttribute?.Name, tableAttribute?.Alias);
                //return e;
            }

            var property = expression.Type.GetProperties().FirstOrDefault(x => string.Equals(x.Name, propertyOrField, StringComparison.InvariantCultureIgnoreCase));
            if (property != null)
            {
                var e =  Expression.Condition(
                    Expression.Equal(expression, Expression.Default(expression.Type)),
                    Expression.Default(property.PropertyType),
                    Expression.PropertyOrField(expression, property.Name));
                return new FieldExpression(e, propertyOrField, tableAttribute?.Name, tableAttribute?.Alias);
                //return e;
            }

            throw new TSQLException($"Field does not exist: {propertyOrField}");
        }

        static public Expression DefaultIfEmpty(this Expression sequence)
        {
            var itemType = sequence.Type.GenericTypeArguments[0];
            var defaultIfEmpty = typeof(Enumerable).GetMethods().First(x => x.Name == "DefaultIfEmpty" && x.GetParameters().Length == 1);
            return Expression.Call(
                null,
                defaultIfEmpty.MakeGenericMethod(itemType),
                new Expression[]
                    {
                    Expression.Convert(sequence, typeof (IEnumerable<>).MakeGenericType(itemType))
                    });

        }

        static public Expression Select(this Expression sequence, Func<ParameterExpression, Expression> selectFunc)
        {
            var secondItemParameter = ParameterExpression.Parameter(sequence.GetElementType(), "secondItemParameter");

            var func = selectFunc(secondItemParameter);
            //"item => new AnonymousType() { SelectProp = item.name, SelectProp2 = item.SelectProp2) }"
            LambdaExpression selectLambda = Expression.Lambda(func, 
                new ParameterExpression[]
                {
                    secondItemParameter
                });

            return sequence.Select(selectLambda);
        }

        static public Expression Select(this Expression sequence, LambdaExpression selectLambda)
        {
            if (sequence.Type.GetGenericTypeDefinition() == typeof(ParallelQuery<>))
            {
                var selectMethods = typeof(ParallelEnumerable)
                    .GetMethods()
                    .Where(x => x.Name == "Select")
                    .Where(x => x.GetGenericArguments().Length == 2)
                    .ToList()
                    .First()
                    .MakeGenericMethod(
                    new Type[] {
                        sequence.GetElementType(),
                        selectLambda.ReturnType
                    });

                var selectMethodsCall = Expression.Call(
                    selectMethods,
                    sequence,
                    selectLambda);

                return selectMethodsCall;
            }
            else
            {

                var selectMethods = typeof(Enumerable)
                    .GetMethods()
                    .Where(x => x.Name == "Select")
                    .Where(x => x.GetGenericArguments().Length == 2)
                    .ToList()
                    .First()
                    .MakeGenericMethod(
                    new Type[] {
                    sequence.GetElementType(),
                    selectLambda.ReturnType
                    });

                var selectMethodsCall = Expression.Call(
                    selectMethods,
                    sequence,
                    selectLambda);

                return selectMethodsCall;
            }
        }

        static public Expression SelectMany(this Expression sequence, Func<ParameterExpression, Expression> selectFunc)
        {
            var selectManyItemParameter = ParameterExpression.Parameter(sequence.GetElementType(), "selectManyItemParameter");

            var funcCall = selectFunc(selectManyItemParameter);
            var selectManyLambda = Expression.Lambda(
                funcCall,
                selectManyItemParameter);

            return sequence.SelectMany(selectManyLambda);
        }
        
        static public Expression SelectMany(this Expression sequence, LambdaExpression selectLambda)
        {
            var selectManyMethods = typeof(ParallelEnumerable)
                .GetMethods()
                .Where(x => x.Name == "SelectMany")
                .Where(x => x.GetGenericArguments().Length == 2)
                .ToList()
                .First()
                .MakeGenericMethod(
                new Type[] {
                    sequence.GetElementType(),
                    selectLambda.ReturnType.GetGenericArguments()[0]
                });

            var selectManyMethodsCall = Expression.Call(
                selectManyMethods,
                sequence,
                selectLambda);

            return selectManyMethodsCall;
        }

        static public Expression Join(this Expression firstSequence, Expression secondSequence, LambdaExpression firstSequenceKeyLambda, LambdaExpression secondSequenceKeyLambda, Func<ParameterExpression, ParameterExpression, Expression> selectFunc)
        {
            var firstItem = ParameterExpression.Parameter(firstSequence.GetElementType(), "firstItem");
            var secondItem = ParameterExpression.Parameter(secondSequence.GetElementType(), "secondItem");

            var func = selectFunc(firstItem, secondItem);

            LambdaExpression selectLambda = Expression.Lambda(func, new ParameterExpression[]
            {
                        firstItem,
                        secondItem,
            });

            return firstSequence.Join(secondSequence, firstSequenceKeyLambda, secondSequenceKeyLambda, selectLambda);
        }

        static public Expression Join(this Expression firstSequence, Expression secondSequence, LambdaExpression firstSequenceKeyLambda, LambdaExpression secondSequenceKeyLambda, LambdaExpression resultLambda)
        {
            var method = typeof(ParallelEnumerable)
                .GetMethods()
                .Where(x => x.Name == "Join")
                .ToList()
                .First()
                .MakeGenericMethod(
                new Type[] {
                    firstSequence.Type.GetGenericArguments()[0],
                    secondSequence.Type.GetGenericArguments()[0],
                    firstSequenceKeyLambda.ReturnType,
                    resultLambda.ReturnType
                });


            var fromCall = Expression.Call(
                method,
                firstSequence,
                secondSequence,
                firstSequenceKeyLambda,
                secondSequenceKeyLambda,
                resultLambda);

            return fromCall;
        }
    
        static public Expression GroupJoin(this Expression firstSequence, Expression secondSequence, LambdaExpression firstSequenceKeyLambda, LambdaExpression secondSequenceKeyLambda, Func<ParameterExpression, ParameterExpression, Expression> selectFunc)
        {
            var firstItem = ParameterExpression.Parameter(firstSequence.GetElementType(), "firstItem");
            var secondItemsList = ParameterExpression.Parameter(typeof(IEnumerable<>).MakeGenericType(secondSequence.GetElementType()), "secondItemsList");

            var func = selectFunc(firstItem, secondItemsList);

            LambdaExpression selectLambda = Expression.Lambda(func, new ParameterExpression[]
            {
                        firstItem,
                        secondItemsList,
            });

            return firstSequence.GroupJoin(secondSequence, firstSequenceKeyLambda, secondSequenceKeyLambda, selectLambda);
        }

        static public Expression GroupJoin(this Expression firstSequence, Expression secondSequence, LambdaExpression firstSequenceKeyLambda, LambdaExpression secondSequenceKeyLambda, LambdaExpression groupLambda)
        {
            var groupJoinMethod = typeof(ParallelEnumerable)
                .GetMethods()
                .Where(x => x.Name == "GroupJoin")
                .ToList()
                .First()
                .MakeGenericMethod(
                new Type[] {
                    firstSequence.GetElementType(),
                    secondSequence.GetElementType(),
                    firstSequenceKeyLambda.ReturnType,
                    groupLambda.ReturnType
                });

            // returns ParallelQuery<FirstSequenceItemType, IEnumerable<SecondSeqequenceItemType>>
            var groupJoinMethodCall = Expression.Call(
                groupJoinMethod,
                firstSequence,
                secondSequence,
                firstSequenceKeyLambda,
                secondSequenceKeyLambda,
                groupLambda);

            return groupJoinMethodCall;
        }

        static public Expression Union(this Expression firstSequence, Expression secondSequence, Expression comparer)
        {
            var method = typeof(ParallelEnumerable)
                .GetMethods()
                .Where(x => x.Name == "Union")
                .Where(x => x.GetParameters().Length == 3)
                .Where(x => x.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == firstSequence.Type.GetGenericTypeDefinition())
                .Where(x => x.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == secondSequence.Type.GetGenericTypeDefinition())
                .FirstOrDefault()
                .MakeGenericMethod(firstSequence.GetElementType());

            if (comparer != null)
            {
                var call = Expression.Call(
                    method,
                    firstSequence,
                    secondSequence,
                    comparer);
                return call;
            }
            else
            {
                var call = Expression.Call(
                    method,
                    firstSequence,
                    secondSequence);
                return call;
            }

        }

        static public Expression Except(this Expression firstSequence, Expression secondSequence, Expression comparer)
        {
            var method = typeof(ParallelEnumerable)
                .GetMethods()
                .Where(x => x.Name == "Except")
                .Where(x => x.GetParameters().Length == 3)
                .Where(x => x.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == firstSequence.Type.GetGenericTypeDefinition())
                .Where(x => x.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == secondSequence.Type.GetGenericTypeDefinition())
                .FirstOrDefault()
                .MakeGenericMethod(firstSequence.GetElementType());

            if (comparer != null)
            {
                var call = Expression.Call(
                    method,
                    firstSequence,
                    secondSequence,
                    comparer);
                return call;
            }
            else
            {
                var call = Expression.Call(
                    method,
                    firstSequence,
                    secondSequence);
                return call;
            }
        }

        static public Expression Concat(this Expression firstSequence, Expression secondSequence)
        {
            var method = typeof(ParallelEnumerable)
                .GetMethods()
                .Where(x => x.Name == "Concat")
                .Where(x => x.GetParameters().Length == 2)
                .Where(x => x.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == firstSequence.Type.GetGenericTypeDefinition())
                .Where(x => x.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == secondSequence.Type.GetGenericTypeDefinition())
                .FirstOrDefault()
                .MakeGenericMethod(firstSequence.GetElementType());

            var call = Expression.Call(
                method,
                firstSequence,
                secondSequence);

            return call;
        }

        static public Expression Intersect(this Expression firstSequence, Expression secondSequence)
        {
            var method = typeof(ParallelEnumerable)
                .GetMethods()
                .Where(x => x.Name == "Intersect")
                .Where(x => x.GetParameters().Length == 2)
                .Where(x => x.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == firstSequence.Type.GetGenericTypeDefinition())
                .Where(x => x.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == secondSequence.Type.GetGenericTypeDefinition())
                .FirstOrDefault()
                .MakeGenericMethod(firstSequence.GetElementType());

            var call = Expression.Call(
                method,
                firstSequence,
                secondSequence);

            return call;
        }

        static public Expression Where(this Expression sequence, Func<ParameterExpression, Expression> predicate)
        {
            var item = ParameterExpression.Parameter(sequence.GetElementType(), "item");
            var func = predicate(item);
            var predicateLambda = Expression.Lambda(func, item);
            return sequence.Where(predicateLambda);
        }
        
        static public Expression Where(this Expression sequence, LambdaExpression predicateLambda)
        {
            MethodCallExpression call = Expression.Call(
                typeof(ParallelEnumerable),
                "Where",
                new Type[] { sequence.GetElementType() },
                sequence,
                predicateLambda);

            return call;
        }

        static public Expression Take(this Expression sequence, int number)
        {
            var takeNumber = Expression.Constant(number);

            MethodCallExpression call = Expression.Call(
                typeof(ParallelEnumerable),
                "Take",
                new Type[] { sequence.GetElementType() },
                sequence,
                takeNumber);

            return call;
        }

        static public Expression Skip(this Expression sequence, int number)
        {
            var takeNumber = Expression.Constant(number);

            MethodCallExpression call = Expression.Call(
                typeof(ParallelEnumerable),
                "Skip",
                new Type[] { sequence.GetElementType() },
                sequence,
                takeNumber);

            return call;
        }

        static public Expression OrderBy(this Expression sequence, Func<Expression, LambdaExpression> predicate)
        {
            var item = ParameterExpression.Parameter(sequence.GetElementType(), "item");
            var lambda = predicate(item);
            return sequence.OrderBy(lambda);
        }

        static public Expression OrderBy(this Expression sequence, LambdaExpression lambdaExpression)
        {
            MethodCallExpression call = Expression.Call(
                           typeof(ParallelEnumerable),
                           "OrderBy",
                           new Type[] { sequence.GetElementType(), lambdaExpression.ReturnType },
                           sequence,
                           lambdaExpression);
            return call;
        }

        static public Expression OrderByDescending(this Expression sequence, Func<Expression, LambdaExpression> predicate)
        {
            var item = ParameterExpression.Parameter(sequence.GetElementType(), "item");
            var lambda = predicate(item);
            return sequence.OrderByDescending(lambda);
        }

        static public Expression OrderByDescending(this Expression sequence, LambdaExpression lambdaExpression)
        {
            MethodCallExpression call = Expression.Call(
               typeof(ParallelEnumerable),
               "OrderByDescending",
               new Type[] { sequence.GetElementType(), lambdaExpression.ReturnType },
               sequence,
               lambdaExpression);
            return call;
        }

        static public Expression ThenBy(this Expression sequence, Func<Expression, LambdaExpression> predicate)
        {
            var item = ParameterExpression.Parameter(sequence.GetElementType(), "item");
            var lambda = predicate(item);
            return sequence.ThenBy(lambda);
        }

        static public Expression ThenBy(this Expression sequence, LambdaExpression lambdaExpression)
        {
            MethodCallExpression call = Expression.Call(
                           typeof(ParallelEnumerable),
                           "ThenBy",
                           new Type[] { sequence.GetElementType(), lambdaExpression.ReturnType },
                           sequence,
                           lambdaExpression);
            return call;
        }

        static public Expression ThenByDescending(this Expression sequence, Func<Expression, LambdaExpression> predicate)
        {
            var item = ParameterExpression.Parameter(sequence.GetElementType(), "item");
            var lambda = predicate(item);
            return sequence.ThenByDescending(lambda);
        }

        static public Expression ThenByDescending(this Expression sequence, LambdaExpression lambdaExpression)
        {
            MethodCallExpression call = Expression.Call(
                           typeof(ParallelEnumerable),
                           "ThenByDescending",
                           new Type[] { sequence.GetElementType(), lambdaExpression.ReturnType },
                           sequence,
                           lambdaExpression);
            return call;
        }

        static public Expression Min(this Expression sequence)
        {
            return Expression.Call(
                 typeof(Enumerable),
                 "Min",
                 new Type[] { },
                 new Expression[] { sequence });
        }

        static public Expression Max(this Expression sequence)
        {
            return Expression.Call(
                 typeof(Enumerable),
                 "Max",
                 new Type[] { },
                 new Expression[] { sequence });
        }

        static public Expression Sum(this Expression sequence)
        {
            return Expression.Call(
                 typeof(Enumerable),
                 "Sum",
                 new Type[] { },
                 new Expression[] { sequence });
        }

        static public Expression Average(this Expression sequence)
        {
            return Expression.Call(
                 typeof(Enumerable),
                 "Average",
                 new Type[] { },
                 new Expression[] { sequence });
        }

        static public Expression Count(this Expression sequence)
        {
            return Expression.Call(
                 typeof(Enumerable),
                 "Count",
                 new Type[] { sequence.GetElementType()},
                 new Expression[] { sequence });
        }

        static public Expression GroupBy(this Expression sequence, LambdaExpression lambdaExpression)
        {
            var call = Expression.Call(
                typeof(ParallelEnumerable),
                "GroupBy",
                new Type[] { sequence.GetElementType(), lambdaExpression.ReturnType },
                sequence,
                lambdaExpression);
            return call;
        }

        static public Expression AsParallel(this Expression sequence)
        {
            //
            Expression call = Expression.Call(
                typeof(ParallelEnumerable),
                "AsParallel",
                new Type[] { sequence.GetElementType() },
                sequence);

            call = Expression.Call(
                typeof(ParallelEnumerable),
                "AsOrdered",
                new Type[] { sequence.GetElementType() },
                call);

            return call;
        }

        static public Expression AsParallel(object sequence, Type itemType)
        {

            Expression call = Expression.Call(
                typeof(ParallelEnumerable),
                "AsParallel",
                new Type[] { itemType },
                Expression.Constant(sequence));

            call = Expression.Call(
                typeof(ParallelEnumerable),
                "AsOrdered",
                new Type[] { itemType },
                call);
            return call;
        }

        static public Expression Contains(this Expression sequence, Expression sequence2)
        {
            return Expression.Call(
                typeof(ParallelEnumerable),
                "Contains",
                new Type[] { sequence.GetElementType() },
                sequence,
                sequence2);
        }

        static public Expression Invoke(this Expression lambda, params Expression[] parameters)
        {
            return Expression.Invoke(lambda, parameters);
        }

        static public Expression WithCancellation(this Expression sequence, CancellationToken ct)
        {
            return Expression.Call(
                    typeof(ParallelEnumerable),
                    "WithCancellation",
                    new Type[] { sequence.GetElementType() },
                    sequence,
                    Expression.Constant(ct));
        }

        static public Expression SelectAs(this Expression sequence, Type outputItemType)
        {
            var inputItemType = GetElementType(sequence);
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
                typeof(ParallelEnumerable),
                "Select",
                new Type[] { inputItemType, outputItemType },
                sequence,
                expression);

            return call;
        }

        static public Expression MapTo(this Expression item, Type type)
        {
            List<MemberBinding> resultBindings = new List<MemberBinding>();
            int fieldIndex = 0;
            foreach (var field in type.GetFields())
            {
                if (item.Type == typeof(object[]))
                {
                    MemberBinding assignment = Expression.Bind(
                        type.GetField(field.Name),
                        Expression.Convert(Expression.ArrayAccess(item, Expression.Constant(fieldIndex)), field.FieldType)
                    );
                    resultBindings.Add(assignment);
                    fieldIndex++;
                }
                else
                {
                    FieldInfo resultItemField = type.GetField(field.Name);
                    Expression resultItemValue = Expression
                        .PropertyOrField(item, field.Name)
                        .ConvertTo(resultItemField.FieldType);

                    //"SelectProp = rowOfDataSource.GetValue(..fieldName..)"
                    MemberBinding assignment = Expression.Bind(
                        resultItemField,
                        resultItemValue
                    );
                    resultBindings.Add(assignment);
                }
            }

            //"new AnonymousType() { SelectProp = item.name, SelectProp2 = item.SelectProp2) }"
            var result = Expression.MemberInit(
                Expression.New(type.GetConstructor(Type.EmptyTypes)),
                resultBindings);

            return result;
        }

        static public Expression ConvertTo(this Expression expression, Type type)
        {
            if (expression.Type == type)
                return expression;

            if (type == typeof(string))
            {
                var toString = typeof(Object).GetMethod("ToString");
                return Expression.Call(expression, toString);
            }

            return Expression.Convert(expression, type);
        }

        static public Expression ConverToNullable(this Expression expression)
        {
            if (expression.Type.IsValueType)
                return Expression.Convert(expression, expression.Type.MakeNullableType());
            return expression;
        }
        
    }

    public class FieldExpression : Expression
    {
        private Expression _inner;
        public string TableName { get; set; }
        public string TableAlias { get; set; }
        public string ColumnName { get; set; }

        public FieldExpression(Expression inner, string fieldName, string tableName, string tableAlias) : base()
        {
            _inner = inner;
            this.TableName = tableName;
            this.TableAlias = tableAlias;
            this.ColumnName = fieldName;
        }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type => _inner.Type;

        public override bool CanReduce => true;

        public override Expression Reduce()
        {
            return _inner;
        }
    }

}