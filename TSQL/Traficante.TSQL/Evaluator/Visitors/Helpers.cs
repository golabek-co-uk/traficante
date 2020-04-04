using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Traficante.TSQL.Evaluator.Visitors
{
    static public class Helpers
    {

        static public Type GetItemType(this Expression sequence)
        {
            return sequence.Type.GetGenericArguments()[0];
        }

        static public Expression PropertyOrField(this Expression expression, string propertyOrField)
        {
            return Expression.PropertyOrField(expression, propertyOrField);
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
            var secondItemParameter = ParameterExpression.Parameter(sequence.GetItemType(), "secondItemParameter");

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
            var selectMethods = typeof(Enumerable)
                .GetMethods()
                .Where(x => x.Name == "Select")
                .Where(x => x.GetGenericArguments().Length == 2)
                .ToList()
                .First()
                .MakeGenericMethod(
                new Type[] {
                    sequence.Type.GetGenericArguments()[0],
                    selectLambda.ReturnType
                });

            var selectMethodsCall = Expression.Call(
                selectMethods,
                sequence,
                selectLambda);

            return selectMethodsCall;
        }

        static public Expression SelectMany(this Expression sequence, Func<ParameterExpression, Expression> selectFunc)
        {
            var selectManyItemParameter = ParameterExpression.Parameter(sequence.GetItemType(), "selectManyItemParameter");

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
                    sequence.Type.GetGenericArguments()[0],
                    selectLambda.ReturnType.GetGenericArguments()[0]
                });

            var selectManyMethodsCall = Expression.Call(
                selectManyMethods,
                sequence,
                selectLambda);

            return selectManyMethodsCall;
        }

        static public Expression Join(this Expression firstSequence, Expression secondSequence, LambdaExpression firstSequenceKeyLambda, LambdaExpression secondSequenceKeyLambda, Func<ParameterExpression, ParameterExpression, Expression> resultLambda)
        {
            return null;
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
            var firstItem = ParameterExpression.Parameter(firstSequence.GetItemType(), "firstItem");
            var secondItemsList = ParameterExpression.Parameter(typeof(IEnumerable<>).MakeGenericType(secondSequence.GetItemType()), "secondItemsList");

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
                    firstSequence.GetItemType(),
                    secondSequence.GetItemType(),
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
    }

}