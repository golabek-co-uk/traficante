using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;
using Traficante.TSQL;
using Traficante.TSQL.Evaluator.Helpers;
using Traficante.TSQL.Evaluator.Utils;
using Traficante.TSQL.Parser;
using Traficante.TSQL.Parser.Nodes;
using Traficante.TSQL.Schema.Managers;
using TextSpan = Traficante.TSQL.Parser.TextSpan;

namespace Traficante.TSQL.Evaluator.Visitors
{
    public class RunQueryVisitor : IExpressionVisitor
    {
        private AnonymousTypeBuilder _typeBuilder = new AnonymousTypeBuilder();
        private Dictionary<string, Expression> _cte = new Dictionary<string, Expression>();
        
        private TSQLEngine _engine;
        private readonly CancellationToken _cancellationToken;

        public Stack<Expression> Nodes { get; set; } = new Stack<Expression>();
        public Stack<ParameterExpression> ScopedParamters { get; set; } = new Stack<ParameterExpression>();
        public Query CurrentQuery { get; set; }
        public QueryPart CurrentQueryPart { get; set; }
        public List<(string Name, Type Type, string ColumnName, string TableName, string TableAlias, Expression Expression, Expression Parameter)> AccessedFields { get; set; } = new List<(string Name, Type Type, string ColumnName, string TableName, string TableAlias, Expression Expression, Expression Parameter)>();


        public object Result = null;

        public RunQueryVisitor(TSQLEngine engine, CancellationToken cancellationToken)
        {
            this._engine = engine;
            this._cancellationToken = cancellationToken;
        }

        public void SetQuery(Query query)
        {
            CurrentQuery = query;
        }

        public void SetQueryPart(QueryPart queryPart)
        {
            CurrentQueryPart = queryPart;
        }


        public void Visit(Node node)
        {
        }

        public void Visit(DescNode node)
        {

            if (node.Type == DescForType.SpecificConstructor)
            {
                //var fromNode = (FromFunctionNode)node.From;

                //var method = _engine.ResolveMethod(fromNode.Function.Name, fromNode.Function.Path, fromNode.Function.ArgumentsTypes);
                //Type itemsType = null;
                //if (typeof(IEnumerable).IsAssignableFrom(method.FunctionMethod.ReturnType))
                //{
                //    itemsType = method.FunctionMethod.ReturnType.GetGenericArguments().FirstOrDefault();
                //}


                //var functionColumns = TypeHelper.GetColumns(itemsType);
                //var descType = expressionHelper.CreateAnonymousType(new (string, Type)[3] {
                //    ("Name", typeof(string)),
                //    ("Index", typeof(int)),
                //    ("Type", typeof(string))
                //});
                
                //var columnsType = typeof(List<>).MakeGenericType(descType);
                //var columns = columnsType.GetConstructors()[0].Invoke(new object[0]);
                //for (int i = 0; i < functionColumns.Length; i++)
                //{
                //    var descObj = descType.GetConstructors()[0].Invoke(new object[0]);
                //    descType.GetField("Name").SetValue(descObj, functionColumns[i].ColumnName);
                //    descType.GetField("Index").SetValue(descObj, functionColumns[i].ColumnIndex);
                //    descType.GetField("Type").SetValue(descObj, functionColumns[i].ColumnType.ToString());
                //    columnsType.GetMethod("Add", new Type[] { descType }).Invoke(columns, new object[] { descObj });
                //}

                //Nodes.Push(Expression.Constant(columns).AsParallel());
                return;
            }
            if (node.Type == DescForType.Schema)
            {
                var fromNode = (FromFunctionNode)node.From;

            }
        }

        public void Visit(StarNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            var returnType = TypeHelper.GetReturnType(left.Type, right.Type);
            right = right.ConvertTo(returnType);
            left = left.ConvertTo(returnType);
            Nodes.Push(Expression.Multiply(left, right));
        }

        public void Visit(FSlashNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            var returnType = TypeHelper.GetReturnType(left.Type, right.Type);
            right = right.ConvertTo(returnType);
            left = left.ConvertTo(returnType);
            Nodes.Push(Expression.Divide(left, right));
        }

        public void Visit(ModuloNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            var returnType = TypeHelper.GetReturnType(left.Type, right.Type);
            right = right.ConvertTo(returnType);
            left = left.ConvertTo(returnType);
            Nodes.Push(Expression.Modulo(left, right));
        }

        public void Visit(AddNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            var returnType = TypeHelper.GetReturnType(left.Type, right.Type);
            right = right.ConvertTo(returnType);
            left = left.ConvertTo(returnType);
            if (node.ReturnType == typeof(string))
            {
                var concatMethod = typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) });
                Nodes.Push(Expression.Add(left, right, concatMethod));
            }
            else
            {
                Nodes.Push(Expression.Add(left, right));
            }
        }

        public void Visit(HyphenNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            var returnType = TypeHelper.GetReturnType(left.Type, right.Type);
            right = right.ConvertTo(returnType);
            left = left.ConvertTo(returnType);
            //(left, right) = this.typeHelper.AlignSimpleTypes(left, right);
            Nodes.Push(Expression.Subtract(left, right));
        }

        public void Visit(AndNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(Expression.And(left, right));
        }

        public void Visit(OrNode node)
        {
            var left = Nodes.Pop();
            var right = Nodes.Pop();
            Nodes.Push(Expression.Or(left, right));
        }

        public void Visit(EqualityNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            
            Nodes.Push(EqualityOperation(left, right, Expression.Equal));
        }

        public void Visit(GreaterOrEqualNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(EqualityOperation(left, right, Expression.GreaterThanOrEqual));
        }

        public void Visit(LessOrEqualNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(EqualityOperation(left, right, Expression.LessThanOrEqual));
        }

        public void Visit(GreaterNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(EqualityOperation(left, right, Expression.GreaterThan));
        }

        public void Visit(LessNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(EqualityOperation(left, right, Expression.LessThan));
        }

        public void Visit(DiffNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(EqualityOperation(left, right, Expression.NotEqual));
        }

        public Expression EqualityOperation(Expression left, Expression right, Func<Expression, Expression, Expression> operation)
        {
            var returnType = TypeHelper.GetReturnType(left.Type, right.Type);
            right = right.ConvertTo(returnType);
            left = left.ConvertTo(returnType);

            var leftIsNotNull = Expression.NotEqual(left, Expression.Default(left.Type));
            var rightIsNotNull = Expression.NotEqual(right, Expression.Default(right.Type));
            var bothAreNotNull = Expression.And(leftIsNotNull, rightIsNotNull);
            return Expression.And(bothAreNotNull, operation(left, right));
        }

        public void Visit(NotNode node)
        {
            Nodes.Push(Expression.Not(Nodes.Pop()));
        }
        public void Visit(LikeNode node)
        {
            //Visit(new FunctionNode(nameof(Operators.Like),
            //    new ArgsListNode(new[] { node.Left, node.Right }),
            //    new string[0],
            //    new MethodInfo { FunctionMethod = typeof(Operators).GetMethod(nameof(Operators.Like)) }));
        }

        public void Visit(RLikeNode node)
        {
            //Visit(new FunctionNode(nameof(Operators.RLike),
            //    new ArgsListNode(new[] { node.Left, node.Right }),
            //    new string[0],
            //    new MethodInfo { FunctionMethod = typeof(Operators).GetMethod(nameof(Operators.RLike)) }));
        }

        public void Visit(InNode node)
        {
            var right = (ArgsListNode)node.Right;
            var left = node.Left;

            var rightExpressions = new List<Expression>();
            for (int i = 0; i < right.Args.Length; i++)
                rightExpressions.Add(Nodes.Pop());

            var leftExpression = Nodes.Pop();

            Expression exp = EqualityOperation(leftExpression, rightExpressions[0], Expression.Equal);
            for (var i = 1; i < rightExpressions.Count; i++)
            {
                exp = Expression.Or(
                    exp,
                    EqualityOperation(leftExpression, rightExpressions[i], Expression.Equal)
                    );
            }

            Nodes.Push(exp);
        }

        private FieldNode _currentFieldNode = null;
        public void SetFieldNode(FieldNode node)
        {
            _currentFieldNode = node;
        }

        public void Visit(FieldNode node)
        {
            if ((node.Expression is AllColumnsNode) == false && CurrentQueryPart == QueryPart.Select)
            {
                CurrentQuery.SelectedFieldsNodes.Add(node);
                var value = Nodes.Pop();
                Nodes.Push(Expression.Convert(value, node.ReturnType));
            }
            /// TODO: add check if conversion is needed
            //var value = Nodes.Pop();
            //Nodes.Push(Expression.Convert(value, node.ReturnType));
            Nodes.Push(Nodes.Pop());
        }

        public void Visit(FieldOrderedNode node)
        {
            var value = Nodes.Pop();
            Nodes.Push(value);
        }

        public void Visit(StringNode node)
        {
            Nodes.Push(Expression.Constant(node.ObjValue, node.ReturnType));
        }

        public void Visit(DecimalNode node)
        {
            Nodes.Push(Expression.Constant(node.ObjValue, node.ReturnType));
        }

        public void Visit(IntegerNode node)
        {
            Nodes.Push(Expression.Constant(node.ObjValue, node.ReturnType));
        }

        public void Visit(BooleanNode node)
        {
            Nodes.Push(Expression.Constant(node.ObjValue, node.ReturnType));
        }

        public void Visit(WordNode node)
        {
            Nodes.Push(Expression.Constant(node.ObjValue, node.ReturnType));
        }

        public void Visit(ContainsNode node)
        {
            var rightNode = node.Right as ArgsListNode;
            var rightArgs = Enumerable.Range(0, rightNode.Args.Length).Select(x => Nodes.Pop());
            var right = Expression.NewArrayInit(rightNode.Args[0].ReturnType, rightArgs);
            var left = Nodes.Pop();
            var result = right
                .AsParallel()
                .Contains(left);

            Nodes.Push(result);
        }

        public void Visit(FunctionNode node)
        {
            var args = Enumerable.Range(0, node.ArgsCount).Select(x => Nodes.Pop()).Reverse().ToArray();
            var argsTypes = args.Select(x => x.Type).ToArray();
            Traficante.TSQL.Schema.Managers.MethodInfo methodInfo = node.Method ?? this._engine.ResolveMethod(node.Name, node.Path, argsTypes);
            node.ChangeMethod(methodInfo);

            if (node.IsAggregateMethod)
            {
                ParameterExpression groupItem = this.ScopedParamters.Pop();
                Expression groupSequence = null;
                if (this.ScopedParamters.Peek().Type.IsGrouping())
                {
                    groupSequence = this.ScopedParamters.Peek();
                    groupSequence = Expression.Convert(
                        groupSequence,
                        typeof(IEnumerable<>).MakeGenericType(groupSequence.Type.GetGroupingElementType()));
                }
                else
                {
                    groupSequence = this.Nodes.Last(x => x.Type.IsSequence());
                }


                if (node.Method.Name == "Count")
                {
                    var selector = Expression.Lambda(args[0], groupItem);
                    Expression count = groupSequence
                        .Select(selector)
                        .Count()
                        .ConverToNullable();
                    Nodes.Push(count);
                    return;
                }
                if (node.Method.Name == "Sum")
                {
                    var selector = Expression.Lambda(args[0], groupItem);
                    Expression sum = groupSequence
                        .Select(selector)
                        .Sum()
                        .ConverToNullable();
                    Nodes.Push(sum);
                    return;
                }
                if (node.Method.Name == "Max")
                {
                    var selector = Expression.Lambda(args[0], groupItem);
                    Expression max = groupSequence
                        .Select(selector)
                        .Max()
                        .ConverToNullable();
                    Nodes.Push(max);
                    return;
                }
                if (node.Method.Name == "Min")
                {
                    var selector = Expression.Lambda(args[0], groupItem);
                    Expression min = groupSequence
                        .Select(selector)
                        .Min()
                        .ConverToNullable();
                    Nodes.Push(min);
                    return;
                }
                if (node.Method.Name == "Avg")
                {
                    var selector = Expression.Lambda(args[0], groupItem);
                    Expression avg = groupSequence
                        .Select(selector)
                        .Average()
                        .ConverToNullable();
                    Nodes.Push(avg);
                    return;
                }
                throw new ApplicationException($"Aggregate method  {node.Method.Name} is not supported.");
            }
            else
            {
                if (string.Equals(node.Name, "RowNumber", StringComparison.InvariantCultureIgnoreCase))
                {
                    var item = this.ScopedParamters.Peek();
                    var itemIndex = this.ScopedParamters.Skip(1).FirstOrDefault();

                    Nodes.Push(Expression.Add(itemIndex, Expression.Constant(1)).ConverToNullable());
                    return;
                }

                if (methodInfo == null)
                    throw new TSQLException($"Function does not exist: {node.Name}");
                var instance = methodInfo.FunctionMethod.ReflectedType.GetConstructors()[0].Invoke(new object[] { });
                /// TODO: check if there can be more that one generic argument
                var method = methodInfo.FunctionMethod;
                if (method.IsGenericMethodDefinition)
                {
                    if (args.Length > 0 && typeof(Type).IsAssignableFrom(argsTypes[0]))
                        method = method.MakeGenericMethod(node.ArgumentsTypes[0]);
                    else
                        method = method.MakeGenericMethod(node.ReturnType);

                }

                var parameters = method.GetParameters();
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i].ParameterType != args[i].Type && parameters[i].ParameterType.IsArray == false) // right.IsArray == false -> because of "params" argument
                    {
                        args[i] = args[i].ConvertTo(parameters[i].ParameterType);
                    }
                }
                var paramsParameter = parameters.FirstOrDefault(x => x.ParameterType.IsArray);
                var paramsParameterIndex = Array.IndexOf(parameters, paramsParameter);
                if (paramsParameter != null)
                {
                    var typeOfParams = paramsParameter.ParameterType.GetElementType();
                    var arrayOfParams = Expression.NewArrayInit(typeOfParams, args.Skip(paramsParameterIndex));
                    args = args.Take(paramsParameterIndex).Concat(new Expression[] { arrayOfParams }).ToArray();
                }

                Nodes.Push(Expression.Call(Expression.Constant(instance), method, args));
            }
        }

        public void Visit(IsNullNode node)
        {
            var currentValue = Nodes.Pop();
            //TODO: check that, Nullable<> is also a value type
            if (currentValue.Type.IsValueType)
            {

                Nodes.Push(Expression.Constant(node.IsNegated));
            }
            else
            {
                var defaultValue = Expression.Default(currentValue.Type);
                if (node.IsNegated)
                {
                    Nodes.Push(Expression.NotEqual(currentValue, defaultValue));
                }
                else
                {
                    Nodes.Push(Expression.Equal(currentValue, defaultValue));
                }
            }
        }

        public void Visit(AccessFieldNode node)
        {
            var fields = this.ScopedParamters
                .SelectMany(x => x.GetFields(new[] { node.Alias, node.Name }))
                .Where(x => x != default)
                .GroupBy(x => x.Expression.Type)
                .Select(x => x.FirstOrDefault())
                .ToList();

            if (fields.Count == 0)
                throw new TSQLException($"Column does not exist: {node.Alias} -> {node.Name}");
            if (fields.Count > 1)
                throw new TSQLException($"Disambiguate column name: {node.Alias} -> {node.Name}");

            this.AccessedFields.Add(fields.FirstOrDefault());
            this.Nodes.Push(fields.FirstOrDefault().Expression);
        }

        public void Visit(AllColumnsNode node)
        {
            int fieldOrder = 0;
            var item = this.ScopedParamters.Peek();
            foreach (var field in item.GetAllFields())
            {
                bool alreadyHasField = this.CurrentQuery.SelectedFieldsNodes.Any(x => string.Equals(x.FieldName, field.Name, StringComparison.OrdinalIgnoreCase));
                if (alreadyHasField == false)
                {
                    fieldOrder++;
                    IdentifierNode identifierNode = new IdentifierNode(field.Name, field.Type);
                    FieldNode fieldNode = new FieldNode(identifierNode, fieldOrder, field.Name);
                    CurrentQuery.SelectedFieldsNodes.Add(fieldNode);
                    Visit(new AccessFieldNode(field.Name, field.TableAlias, field.Type, TextSpan.Empty));
                }
            }
        }

        public void Visit(IdentifierNode node)
        {
            var fields = this.ScopedParamters
                .SelectMany(x => x.GetFields(new[] { node.Name }))
                .Where(x => x != default)
                .GroupBy(x => x.Expression.Type)
                .Select(x => x.FirstOrDefault())
                .ToList();
            if (fields.Count == 0)
                throw new TSQLException($"Column does not exist: {node.Name}");

            var fieldsWithEmptyAlias = fields.Where(x => string.IsNullOrEmpty(x.TableAlias)).ToList();
            if (fieldsWithEmptyAlias.Count == 1)
            {
                var field = fieldsWithEmptyAlias.FirstOrDefault();
                node.ChangeReturnType(field.Type);
                this.AccessedFields.Add(field);
                this.Nodes.Push(field.Expression);
                return;
            }
            if (fields.Count == 1)
            {
                var field = fields.FirstOrDefault();
                node.ChangeReturnType(field.Type);
                this.AccessedFields.Add(field);
                this.Nodes.Push(field.Expression);
                return;
            }

            throw new TSQLException($"Disambiguate column name: {node.Name}");
        }


        public void Visit(AccessArrayFieldNode node)
        {
            var fields = this.ScopedParamters
                .SelectMany(x => x.GetFields(new[] { node.Name }))
                .Where(x => x != default)
                .GroupBy(x => x.Expression.Type)
                .Select(x => x.FirstOrDefault())
                .ToList();
            if (fields.Count == 0)
                throw new TSQLException($"Column does not exist: {node.Name}");

            var fieldsWithEmptyAlias = fields.Where(x => string.IsNullOrEmpty(x.TableAlias)).ToList();
            if (fieldsWithEmptyAlias.Count == 1)
            {
                var field = fieldsWithEmptyAlias.FirstOrDefault();
                field.Expression = Expression.ArrayAccess(field.Expression, Expression.Constant(node.Token.Index)).ConverToNullable();
                field.Type = field.Expression.Type;
                node.ChangeReturnType(field.Type);
                this.AccessedFields.Add(field);
                this.Nodes.Push(field.Expression);
                return;
            }
            if (fields.Count == 1)
            {
                var field = fields.FirstOrDefault();
                field.Expression = Expression.ArrayAccess(field.Expression, Expression.Constant(node.Token.Index)).ConverToNullable();
                field.Type = field.Expression.Type;
                node.ChangeReturnType(field.Type);
                this.AccessedFields.Add(field);
                this.Nodes.Push(field.Expression);
                return;
            }

            throw new TSQLException($"Disambiguate column name: {node.Name}");
        }

        public void Visit(AccessObjectKeyNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(PropertyValueNode node)
        {
            var obj = Nodes.Pop();
            Nodes.Push(Expression.Property(obj, node.PropertyInfo));
        }

        public void Visit(VariableNode node)
        {
            var variable = _engine.GetVariable(node.Name);
            Nodes.Push(Expression.Constant(variable?.Value, variable?.Type ?? node.ReturnType));
        }

        public void Visit(DeclareNode node)
        {
            if (node.Value != null)
            {
                Expression valueExpression = Nodes.Pop();
                valueExpression = valueExpression.ConvertTo(node.Type.ReturnType).ConvertTo(typeof(object));
                var value = Expression.Lambda<Func<object>>(valueExpression).Compile()();
                _engine.SetVariable(node.Variable.Name, node.Type.ReturnType, value);
                return;
            }
            _engine.SetVariable(node.Variable.Name, node.Type.ReturnType, null);
        }

        public void Visit(SetNode node)
        {
            Expression valueExpression = Nodes.Pop();
            var variableType = _engine.GetVariable(node.Variable.Name).Type;
            valueExpression = valueExpression.ConvertTo(variableType).ConvertTo(typeof(object));
            var value = Expression.Lambda<Func<object>>(valueExpression).Compile()();
            _engine.SetVariable(node.Variable.Name, value);
        }

        public void Visit(DotNode node)
        {
            if (node.Expression is FunctionNode)
            {
                List<string> path = new List<string>();
                Node parentNode = node.Root;
                while (parentNode is null == false)
                {
                    if (parentNode is IdentifierNode)
                        path.Add(((IdentifierNode)parentNode).Name);
                    if (parentNode is PropertyValueNode)
                        path.Add(((PropertyValueNode)parentNode).Name);
                    if (parentNode is DotNode)
                    {
                        var dot = (DotNode)parentNode;
                        if (dot.Expression is IdentifierNode)
                            path.Add(((IdentifierNode)dot.Expression).Name);
                        if (parentNode is PropertyValueNode)
                            path.Add(((PropertyValueNode)dot.Expression).Name);
                    }
                    parentNode = (parentNode as DotNode)?.Root;
                }

                path.Reverse();

                FunctionNode function = node.Expression as FunctionNode;
                function.ChangePath(path.ToArray());
                Visit(function);
                return;
            }

            if (node.Expression is IdentifierNode identifierNode)
            {
                List<string> path = new List<string>();
                Node parentNode = node.Root;
                while (parentNode is null == false)
                {
                    if (parentNode is IdentifierNode)
                        path.Add(((IdentifierNode)parentNode).Name);
                    if (parentNode is PropertyValueNode)
                        path.Add(((PropertyValueNode)parentNode).Name);
                    if (parentNode is DotNode)
                    {
                        var dot = (DotNode)parentNode;
                        if (dot.Expression is IdentifierNode)
                            path.Add(((IdentifierNode)dot.Expression).Name);
                        if (parentNode is PropertyValueNode)
                            path.Add(((PropertyValueNode)dot.Expression).Name);
                    }
                    parentNode = (parentNode as DotNode)?.Root;
                }
                path.Reverse();
                path.Add(identifierNode.Name);

                var fields = this.ScopedParamters
                    .SelectMany(x => x.GetFields(path.ToArray()))
                    .Where(x => x != default)
                    .GroupBy(x => x.Expression.Type)
                    .Select(x => x.FirstOrDefault())
                    .ToList();

                if (fields.Count == 0)
                    throw new TSQLException($"Column does not exist: {string.Join(" -> ", path)}");
                if (fields.Count > 1)
                    throw new TSQLException($"Disambiguate column: {string.Join(" -> ", path)}");

                var field = fields.FirstOrDefault();
                this.AccessedFields.Add(field);

                if (identifierNode is AccessArrayFieldNode accessObjectArray)
                {
                    field.Expression = Expression.ArrayAccess(field.Expression, Expression.Constant(accessObjectArray.Token.Index)).ConverToNullable();
                    field.Type = field.Expression.Type;
                }

                identifierNode.ChangeReturnType(field.Type);
                this.Nodes.Push(field.Expression);
                return;
            }
        }


        public void Visit(ArgsListNode node)
        {
            //Nodes.Push(node);
        }

        public void Visit(SelectNode node)
        {
            var selectedFieldsNodes = CurrentQuery.SelectedFieldsNodes;

            var fieldNodes = new Expression[selectedFieldsNodes.Count];
            for (var i = 0; i < selectedFieldsNodes.Count; i++)
                fieldNodes[selectedFieldsNodes.Count - 1 - i] = Nodes.Pop();


            var outputItemType = _typeBuilder.CreateAnonymousType(selectedFieldsNodes.Select(x => (x.FieldName, x.ReturnType)).Distinct());

            List<MemberBinding> bindings = new List<MemberBinding>();
            for (int i = 0; i < selectedFieldsNodes.Count; i++)
            {
                var name = selectedFieldsNodes[i].FieldName;
                var value = fieldNodes[i];
                //"SelectProp = inputItem.Prop"
                MemberBinding assignment = Expression.Bind(
                    outputItemType.GetField(name),
                    value);
                bindings.Add(assignment);
            }

            //"new AnonymousType()"
            var creationExpression = Expression.New(outputItemType.GetConstructor(Type.EmptyTypes));

            //"new AnonymousType() { SelectProp = item.name, SelectProp2 = item.SelectProp2) }"
            var initialization = Expression.MemberInit(creationExpression, bindings);

            if (CurrentQuery == null || CurrentQuery.HasFromClosure() == false)
            {
                var array = Expression.NewArrayInit(outputItemType, new Expression[] { initialization });
                var sequence = array.AsParallel();
                var lambda = Expression.Lambda(sequence);
                Nodes.Push(lambda.Invoke());
                return;
            }

            if (CurrentQuery.IsSingleRowResult())
            {
                var item = this.ScopedParamters.Pop();
                var item_i = this.ScopedParamters.Pop();
                var sequence = this.Nodes.Pop();

                var array = Expression.NewArrayInit(outputItemType, new Expression[] { initialization });
                sequence = array.AsParallel();
                var lambda = Expression.Lambda(sequence);
                Nodes.Push(lambda.Invoke());
            }
            else
            {
                var item = this.ScopedParamters.Pop();
                var item_i = this.ScopedParamters.Pop();
                var sequence = this.Nodes.Pop();

                //"item => new AnonymousType() { SelectProp = item.name, SelectProp2 = item.SelectProp2) }"
                Expression expression = Expression.Lambda(initialization, item, item_i);

                
                sequence = Expression.Call(
                    typeof(ParallelEnumerable),
                    "Select",
                    new Type[] { item.Type, outputItemType },
                    sequence,
                    expression);

                Nodes.Push(sequence);
            }


        }

        public void Visit(WhereNode node)
        {
            var item_i = this.ScopedParamters.Pop();
            var item = this.ScopedParamters.Pop();

            var predicate = Nodes.Pop();
            var predicateLambda = Expression.Lambda(predicate, item);

            var sequence = Nodes.Pop();
            sequence = sequence.Where(predicateLambda);
            Nodes.Push(sequence);

        }

        public void Visit(GroupByNode node)
        {
            var item = this.ScopedParamters.Pop();

            var outputFields = new (FieldNode Field, Expression Value)[node.Fields.Length];
            for (var i = 0; i < node.Fields.Length; i++)
                outputFields[node.Fields.Length - 1 - i] = (node.Fields[node.Fields.Length - 1 - i], Nodes.Pop());

            var outputItemType = _typeBuilder.CreateAnonymousType(
                outputFields.Select<(FieldNode Field, Expression Value),(string, Type ReturnType, string ColumnName, string TableName, string TableAlias)>(x => (
                    x.Field.FieldName,
                    x.Field.ReturnType,
                    (x.Value as FieldExpression)?.ColumnName,
                    (x.Value as FieldExpression)?.TableName,
                    (x.Value as FieldExpression)?.TableAlias)),
                tableName: string.Empty,
                tableAlias: string.Empty);
            var sequence = this.Nodes.Pop();

            List<MemberBinding> bindings = new List<MemberBinding>();
            foreach (var field in outputFields)
            {
                //"SelectProp = inputItem.Prop"
                MemberBinding assignment = Expression.Bind(
                    outputItemType.GetField(field.Field.FieldName),
                    field.Value);
                bindings.Add(assignment);
            }

            //"new AnonymousType()"
            var creationExpression = Expression.New(outputItemType.GetConstructor(Type.EmptyTypes));

            //"new AnonymousType() { SelectProp = item.name, SelectProp2 = item.SelectProp2) }"
            var initialization = Expression.MemberInit(creationExpression, bindings);

            //"item => new AnonymousType() { SelectProp = item.name, SelectProp2 = item.SelectProp2) }"
            LambdaExpression lambdaPredicate = Expression.Lambda(initialization, item);

            sequence = sequence.GroupBy(lambdaPredicate);

            Nodes.Push(sequence);
        }

        public void Visit(HavingNode node)
        {
            var predicate = Nodes.Pop();
            var sequence = Nodes.Pop();
            var item = this.ScopedParamters.Pop();
            var predicateLambda = Expression.Lambda(predicate, item);
            sequence = sequence.Where(predicateLambda);
            Nodes.Push(sequence);
        }

        public void Visit(SkipNode node)
        {
            var sequence = this.Nodes.Pop();
            sequence = sequence.Skip((int)node.Value);
            Nodes.Push(sequence);
        }

        public void Visit(TakeNode node)
        {
            var sequence = this.Nodes.Pop();
            sequence = sequence.Take((int)node.Value);
            Nodes.Push(sequence);
        }

        public void Visit(TopNode node)
        {
            var sequence = this.Nodes.Pop();
            sequence = sequence.Take((int)node.Value);
            Nodes.Push(sequence);
        }

        public IEnumerable<Object[]> AsEnumerable(IDataReader source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            List<Object[]> list = new List<object[]>();
            while (source.Read())
            {
                Object[] row = new Object[source.FieldCount];
                source.GetValues(row);
                for (int i = 0; i < source.FieldCount; i++)
                {
                    if (row[i] is DBNull)
                        row[i] = GetDefaultValue(source.GetFieldType(i));
                }
                //yield return row;
                list.Add(row);
            }
            return list;
        }

        public object GetDefaultValue(Type t)
        {
            if (t.IsValueType && Nullable.GetUnderlyingType(t) == null)
            {
                return Activator.CreateInstance(t);
            }
            else
            {
                return null;
            }
        }

        public void Visit(FromFunctionNode node)
        {
            var function = node.Function;
            var method = _engine.ResolveMethod(function.Name, function.Path, function.ArgumentsTypes);
            if (method == null)
                throw new TSQLException($"Cannot resolve function \"{function.Name}\" under path \"{string.Join(".", function.Path.Select(p =>$"[{p}]"))}\"  with arguments \"{string.Join(", ", function.ArgumentsTypes.Select(x => x.Name))}\"");

            List<Expression> functionExpressionArgumetns = new List<Expression>();
            for (int i = 0; i < function.ArgsCount; i++)
                functionExpressionArgumetns.Add(this.Nodes.Pop());
            functionExpressionArgumetns.Reverse();
            var callFunction = Expression.Call(Expression.Constant(method.FunctionDelegate.Target), method.FunctionMethod, functionExpressionArgumetns);

            var resultAsObjectExpression = Expression.Convert(callFunction, typeof(object));
            var result = Expression.Lambda<Func<object>>(resultAsObjectExpression).Compile()();

            var resultType = result.GetType();
            var resultItemsType = result.GetType().GetElementType();
            Expression sequence = null;

            List<(string Name, Type FieldType)> resultFields = null;
            if (resultItemsType != null)
            {
                resultFields = resultItemsType.GetProperties().Select(x => (x.Name, x.PropertyType)).ToList();
                sequence = ExpressionHelpers.AsParallel(result, resultItemsType);
            }
            else if (typeof(IAsyncDataReader).IsAssignableFrom(resultType))
            {
                var resultReader = (IAsyncDataReader)result;
                resultItemsType = typeof(object[]);
                resultFields = Enumerable
                    .Range(0, resultReader.FieldCount)
                    .Select(x => (resultReader.GetName(x), resultReader.GetFieldType(x)))
                    .ToList();

                sequence = ExpressionHelpers.AsParallel(
                    new AsyncDataReaderEnumerable(resultReader, this._cancellationToken),
                    resultItemsType);
            }
            else if (typeof(IDataReader).IsAssignableFrom(resultType))
            {
                var resultReader = (IDataReader)result;
                resultItemsType = typeof(object[]);
                resultFields = Enumerable
                    .Range(0, resultReader.FieldCount)
                    .Select(x => (resultReader.GetName(x), resultReader.GetFieldType(x)))
                    .ToList();

                sequence = ExpressionHelpers.AsParallel(
                    new DataReaderEnumerable(resultReader, this._cancellationToken),
                    resultItemsType);
            }


            sequence = sequence.Select(resultItemExpression =>
            {
                Type resultItemType = _typeBuilder.CreateAnonymousType(resultFields, null, node.Alias);
                return resultItemExpression.MapTo(resultItemType);
            });

            Nodes.Push(sequence);
        }

        public void Visit(FromTableNode node)
        {
            if (_cte.ContainsKey(node.Table.TableOrView))
            {
                Visit(new InMemoryTableFromNode(node.Table.TableOrView, node.Alias));
                return;
            }

            var tableData = this._engine.DataManager.GeTable(node.Table.TableOrView, node.Table.Path).Result;
            Expression sequence = ExpressionHelpers.AsParallel(tableData.Results, tableData.ResultItemsType);
            sequence = sequence.Select(resultItemExpression =>
            {
                Type resultItemType = _typeBuilder.CreateAnonymousType(tableData.ResultFields, node.Table.TableOrView, node.Alias);
                return resultItemExpression.MapTo(resultItemType);
            });
            

            Nodes.Push(sequence);
        }

        public void Visit(InMemoryTableFromNode node)
        {
            var table = _cte[node.VariableName];
            table = table.SelectAs(this._typeBuilder.CreateAnonymousTypeSameAs(table.GetElementType(), node.VariableName, null));
            Nodes.Push(table);
        }

        public void Visit(ExpressionFromNode node)
        {
        }

        public void Visit(IntoNode node)
        {
        }

        public void Visit(QueryScope node)
        {
        }

        public void Visit(QueryNode node)
        {
       
        }

        public void Visit(RootNode node)
        {
            if (Nodes.Any())
            {
                Expression last = Nodes.Pop();
                last = last.WithCancellation(this._cancellationToken);
                Expression<Func<object>> toStream = Expression.Lambda<Func<object>>(last);
                var compiledToStream = toStream.Compile();
                Result = compiledToStream();
            }
        }

        public void Visit(SingleSetNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(UnionNode node)
        {
            var secondSequence = Nodes.Pop();
            var firstSequence = Nodes.Pop();

            var outputItemType = _typeBuilder.CreateAnonymousTypeSameAs(firstSequence.GetElementType());
            firstSequence = firstSequence.SelectAs(outputItemType);
            secondSequence = secondSequence.SelectAs(outputItemType);

            Expression comparer = null;
            if (node.Keys.Length > 0)
            {
                var comparerType = _typeBuilder.CreateEqualityComparerForType(outputItemType, node.Keys);
                comparer = Expression.New(comparerType);
            }

            var call = firstSequence.Union(secondSequence, comparer);

            Nodes.Push(call);

        }

        public void Visit(UnionAllNode node)
        {
            var secondSequence = Nodes.Pop();
            var firstSequence = Nodes.Pop();

            var outputItemType = _typeBuilder.CreateAnonymousTypeSameAs(firstSequence.GetElementType());
            firstSequence = firstSequence.SelectAs(outputItemType);
            secondSequence = secondSequence.SelectAs(outputItemType);

            var call = firstSequence.Concat(secondSequence);

            Nodes.Push(call);
        }

        public void Visit(ExceptNode node)
        {
            var secondSequence = Nodes.Pop();
            var firstSequence = Nodes.Pop();

            var outputItemType = _typeBuilder.CreateAnonymousTypeSameAs(firstSequence.GetElementType());
            firstSequence = firstSequence.SelectAs(outputItemType);
            secondSequence = secondSequence.SelectAs(outputItemType);

            Expression comparer = null;
            if (node.Keys.Length > 0)
            {
                var comparerType = _typeBuilder.CreateEqualityComparerForType(outputItemType, node.Keys);
                comparer = Expression.New(comparerType);
            }

            var call = firstSequence.Except(secondSequence, comparer);

            Nodes.Push(call);
        }

        public void Visit(IntersectNode node)
        {
            var secondSequence = Nodes.Pop();
            var firstSequence = Nodes.Pop();

            var outputItemType = _typeBuilder.CreateAnonymousTypeSameAs(firstSequence.GetElementType());
            firstSequence = firstSequence.SelectAs(outputItemType);
            secondSequence = secondSequence.SelectAs(outputItemType);

            var call = firstSequence.Intersect(secondSequence);

            Nodes.Push(call);
        }

        public void Visit(PutTrueNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(MultiStatementNode node)
        {
        }

        public void Visit(StatementsArrayNode node)
        {

        }

        public void Visit(StatementNode node)
        {

        }

        public void Visit(CteExpressionNode node)
        {
        }

        public void Visit(CteInnerExpressionNode node)
        {
            _cte[node.Name] = Nodes.Pop();
        }

        public void Visit(JoinNode node)
        {
            if (node.JoinOperator == JoinOperator.Hash)
            {
                if (node.JoinType == JoinType.Inner)
                    VisitHashInnerJoin(node);
                if (node.JoinType == JoinType.OuterLeft)
                    VisitHashLeftJoin(node);
            }
            else
            {
                if (node.JoinType == JoinType.Inner)
                    VisitLoopInnerJoin(node);
                if (node.JoinType == JoinType.OuterLeft)
                    VisitLoopLeftJoin(node);
            }
        }

        public void VisitHashLeftJoin(JoinNode node)
        {
            var onNode = ((EqualityNode)node.Expression);

            var secondSequenceKeyExpression = this.Nodes.Pop();
            var firstSequenceKeyExpression = this.Nodes.Pop();

            var secondSequence = this.Nodes.Pop();
            var secondSequenceAlias = node.With.Alias;
            var secondSequenceItem = this.ScopedParamters.Pop();
            var secondSequenceKeyLambda = Expression.Lambda(secondSequenceKeyExpression, secondSequenceItem);

            var firstSequence = this.Nodes.Pop();
            var firstSequenceAlias = node.Source.Alias;
            var firstSequenceItem = this.ScopedParamters.Pop();
            var firstSequenceKeyLambda = Expression.Lambda(firstSequenceKeyExpression, firstSequenceItem);

            bool isFirstJoin = (node.Source is JoinNode) == false;

            var groupJoin = firstSequence.HashGroupJoin(
                secondSequence,
                firstSequenceKeyLambda,
                secondSequenceKeyLambda,
                //FirstSequenceItemType firstItem, IEnumerable<SecondSequenceItemType> secondItemsList
                (firstItem, secondItemsList) =>
                {
                    var returnType = this._typeBuilder.CreateAnonymousType(new (string Alias, Type Type)[]
                    {
                        (firstSequenceAlias, firstItem.Type),
                        (secondSequenceAlias, secondItemsList.Type)
                    });

                    var newItem = Expression.MemberInit(
                        Expression.New(returnType.GetConstructor(Type.EmptyTypes)),
                        new List<MemberBinding>()
                        {
                            Expression.Bind(returnType.GetField(firstSequenceAlias), firstItem),
                            Expression.Bind(returnType.GetField(secondSequenceAlias), secondItemsList.DefaultIfEmpty())
                        });

                    return newItem;
                });


 
            //<FirstSequenceItemType, IEnumerable<SecondSeqequenceItemType>> selectManyItemParameter
            var selectManyMethodsCall = groupJoin.SelectMany((groupItem) =>
            {
                var firstItem = Expression.PropertyOrField(groupItem, firstSequenceAlias);
                var selectMethodsCall = groupItem.PropertyOrField(secondSequenceAlias)
                    .Select((secondItem) =>
                    {
                        //SecondSeqequenceItemType inTheGroup
                        
                        List<(string Alias, Type Type)> returnFields = new List<(string Alias, Type Type)>();
                        List<(string Alias, Expression Value)> returnFieldsBindings = new List<(string Alias, Expression Value)>();


                        if (isFirstJoin)
                        {
                            returnFields.Add((firstSequenceAlias, firstSequence.GetElementType()));
                            returnFieldsBindings.Add((firstSequenceAlias, firstItem));
                        }
                        else
                        {
                            foreach(var field in firstSequence.GetElementType().GetFields())
                            {
                                returnFields.Add((field.Name, field.FieldType));
                                returnFieldsBindings.Add((field.Name, Expression.PropertyOrField(firstItem, field.Name)));
                            }
                        }
                        returnFields.Add((secondSequenceAlias, secondSequence.GetElementType()));
                        returnFieldsBindings.Add((secondSequenceAlias, secondItem));

                        var returnType = this._typeBuilder.CreateAnonymousType(returnFields.ToArray());
                        List<MemberBinding> resultBindings = new List<MemberBinding>();
                        //"SelectProp = inputItem.Prop"
                        foreach (var binding in returnFieldsBindings)
                        {
                            resultBindings.Add(Expression.Bind(returnType.GetField(binding.Alias), binding.Value));
                        }
                        var createResultInstance = Expression.MemberInit(
                            Expression.New(returnType.GetConstructor(Type.EmptyTypes)),
                            resultBindings);

                        return createResultInstance;


                    });

                return selectMethodsCall;

            });

            Nodes.Push(selectManyMethodsCall);
        }

        public void VisitHashInnerJoin(JoinNode node)
        {
            var onNode = ((EqualityNode)node.Expression);

            var secondSequenceKeyExpression = this.Nodes.Pop();
            var firstSequenceKeyExpression = this.Nodes.Pop();

            var secondSequence = this.Nodes.Pop();
            var secondSequenceItem = this.ScopedParamters.Pop();
            var secondSequenceAlias = new string[] {
                    node.With.Alias,
                    secondSequenceItem.Type.GetTableAttribute()?.Alias,
                    secondSequenceItem.Type.GetTableAttribute()?.Name }
                .FirstOrDefault(s => !string.IsNullOrEmpty(s));
            var secondSequenceKeyLambda = Expression.Lambda(secondSequenceKeyExpression, secondSequenceItem);
            
            var firstSequence = this.Nodes.Pop();
            var firstSequenceItem = this.ScopedParamters.Pop();
            var firstSequenceAlias
                 = new string[] {
                    node.Source.Alias,
                    firstSequenceItem.Type.GetTableAttribute()?.Alias,
                    firstSequenceItem.Type.GetTableAttribute()?.Name }
                .FirstOrDefault(s => !string.IsNullOrEmpty(s));
            var firstSequenceKeyLambda = Expression.Lambda(firstSequenceKeyExpression, firstSequenceItem);

            bool isFirstJoin = (node.Source is JoinNode) == false;

            var join = firstSequence.HashJoin(
                secondSequence,
                firstSequenceKeyLambda,
                secondSequenceKeyLambda,
                (firstItem, secondItem) =>
                {
                    List<(string Alias, Type Type)> returnFields = new List<(string Alias, Type Type)>();
                    List<(string Alias, Expression Value)> returnFieldsBindings = new List<(string Alias, Expression Value)>();


                    if (isFirstJoin)
                    {
                        returnFields.Add((firstSequenceAlias, firstSequence.GetElementType()));
                        returnFieldsBindings.Add((firstSequenceAlias, firstItem));
                    }
                    else
                    {
                        foreach (var field in firstSequence.GetElementType().GetFields())
                        {
                            returnFields.Add((field.Name, field.FieldType));
                            returnFieldsBindings.Add((field.Name, Expression.PropertyOrField(firstItem, field.Name)));
                        }
                    }
                    returnFields.Add((secondSequenceAlias, secondSequence.GetElementType()));
                    returnFieldsBindings.Add((secondSequenceAlias, secondItem));

                    var returnType = this._typeBuilder.CreateAnonymousType(returnFields.ToArray());
                    List<MemberBinding> resultBindings = new List<MemberBinding>();
                    //"SelectProp = inputItem.Prop"
                    foreach (var binding in returnFieldsBindings)
                    {
                        resultBindings.Add(Expression.Bind(returnType.GetField(binding.Alias), binding.Value));
                    }
                    var createResultInstance = Expression.MemberInit(
                        Expression.New(returnType.GetConstructor(Type.EmptyTypes)),
                        resultBindings);

                    return createResultInstance;
                }
            );

            Nodes.Push(join);
        }

        public void VisitLoopLeftJoin(JoinNode node)
        {
            var onExpression = this.Nodes.Pop();

            var secondSequence = this.Nodes.Pop();
            var secondSequenceAlias = node.With.Alias;
            var secondSequenceItem = this.ScopedParamters.Pop();

            var firstSequence = this.Nodes.Pop();
            var firstSequenceAlias = node.Source.Alias;
            var firstSequenceItem = this.ScopedParamters.Pop();

            var onLambda = Expression.Lambda(onExpression, firstSequenceItem, secondSequenceItem);

            bool isFirstJoin = (node.Source is JoinNode) == false;

            var groupJoin = firstSequence.LoopGroupJoin(
                secondSequence,
                onLambda,
                //FirstSequenceItemType firstItem, IEnumerable<SecondSequenceItemType> secondItemsList
                (firstItem, secondItemsList) =>
                {
                    var returnType = this._typeBuilder.CreateAnonymousType(new (string Alias, Type Type)[]
                    {
                        (firstSequenceAlias, firstItem.Type),
                        (secondSequenceAlias, secondItemsList.Type)
                    });

                    var newItem = Expression.MemberInit(
                        Expression.New(returnType.GetConstructor(Type.EmptyTypes)),
                        new List<MemberBinding>()
                        {
                            Expression.Bind(returnType.GetField(firstSequenceAlias), firstItem),
                            Expression.Bind(returnType.GetField(secondSequenceAlias), secondItemsList.DefaultIfEmpty())
                        });

                    return newItem;
                });



            //<FirstSequenceItemType, IEnumerable<SecondSeqequenceItemType>> selectManyItemParameter
            var selectManyMethodsCall = groupJoin.SelectMany((groupItem) =>
            {
                var firstItem = Expression.PropertyOrField(groupItem, firstSequenceAlias);
                var selectMethodsCall = groupItem.PropertyOrField(secondSequenceAlias)
                    .Select((secondItem) =>
                    {
                        //SecondSeqequenceItemType inTheGroup

                        List<(string Alias, Type Type)> returnFields = new List<(string Alias, Type Type)>();
                        List<(string Alias, Expression Value)> returnFieldsBindings = new List<(string Alias, Expression Value)>();


                        if (isFirstJoin)
                        {
                            returnFields.Add((firstSequenceAlias, firstSequence.GetElementType()));
                            returnFieldsBindings.Add((firstSequenceAlias, firstItem));
                        }
                        else
                        {
                            foreach (var field in firstSequence.GetElementType().GetFields())
                            {
                                returnFields.Add((field.Name, field.FieldType));
                                returnFieldsBindings.Add((field.Name, Expression.PropertyOrField(firstItem, field.Name)));
                            }
                        }
                        returnFields.Add((secondSequenceAlias, secondSequence.GetElementType()));
                        returnFieldsBindings.Add((secondSequenceAlias, secondItem));

                        var returnType = this._typeBuilder.CreateAnonymousType(returnFields.ToArray());
                        List<MemberBinding> resultBindings = new List<MemberBinding>();
                        //"SelectProp = inputItem.Prop"
                        foreach (var binding in returnFieldsBindings)
                        {
                            resultBindings.Add(Expression.Bind(returnType.GetField(binding.Alias), binding.Value));
                        }
                        var createResultInstance = Expression.MemberInit(
                            Expression.New(returnType.GetConstructor(Type.EmptyTypes)),
                            resultBindings);

                        return createResultInstance;


                    });

                return selectMethodsCall;

            });

            Nodes.Push(selectManyMethodsCall);
        }

        public void VisitLoopInnerJoin(JoinNode node)
        {
            var onExpression = this.Nodes.Pop();

            var secondSequence = this.Nodes.Pop();
            var secondSequenceItem = this.ScopedParamters.Pop();
            var secondSequenceAlias = new string[] {
                    node.With.Alias,
                    secondSequenceItem.Type.GetTableAttribute()?.Alias,
                    secondSequenceItem.Type.GetTableAttribute()?.Name }
                .FirstOrDefault(s => !string.IsNullOrEmpty(s));
            //var secondSequenceKeyLambda = Expression.Lambda(secondSequenceKeyExpression, secondSequenceItem);

            var firstSequence = this.Nodes.Pop();
            var firstSequenceItem = this.ScopedParamters.Pop();
            var firstSequenceAlias
                 = new string[] {
                    node.Source.Alias,
                    firstSequenceItem.Type.GetTableAttribute()?.Alias,
                    firstSequenceItem.Type.GetTableAttribute()?.Name }
                .FirstOrDefault(s => !string.IsNullOrEmpty(s));
            
            var onLambda = Expression.Lambda(onExpression, firstSequenceItem, secondSequenceItem);

            bool isFirstJoin = (node.Source is JoinNode) == false;

            var join = firstSequence.LoopJoin(
                secondSequence,
                onLambda,
                (firstItem, secondItem) =>
                {
                    List<(string Alias, Type Type)> returnFields = new List<(string Alias, Type Type)>();
                    List<(string Alias, Expression Value)> returnFieldsBindings = new List<(string Alias, Expression Value)>();


                    if (isFirstJoin)
                    {
                        returnFields.Add((firstSequenceAlias, firstSequence.GetElementType()));
                        returnFieldsBindings.Add((firstSequenceAlias, firstItem));
                    }
                    else
                    {
                        foreach (var field in firstSequence.GetElementType().GetFields())
                        {
                            returnFields.Add((field.Name, field.FieldType));
                            returnFieldsBindings.Add((field.Name, Expression.PropertyOrField(firstItem, field.Name)));
                        }
                    }
                    returnFields.Add((secondSequenceAlias, secondSequence.GetElementType()));
                    returnFieldsBindings.Add((secondSequenceAlias, secondItem));

                    var returnType = this._typeBuilder.CreateAnonymousType(returnFields.ToArray());
                    List<MemberBinding> resultBindings = new List<MemberBinding>();
                    //"SelectProp = inputItem.Prop"
                    foreach (var binding in returnFieldsBindings)
                    {
                        resultBindings.Add(Expression.Bind(returnType.GetField(binding.Alias), binding.Value));
                    }
                    var createResultInstance = Expression.MemberInit(
                        Expression.New(returnType.GetConstructor(Type.EmptyTypes)),
                        resultBindings);

                    return createResultInstance;
                }
            );

            Nodes.Push(join);
        }

        public void Visit(OrderByNode node)
        {
            var item = this.ScopedParamters.Pop();

            var fieldNodes = new Expression[node.Fields.Length];
            for (var i = 0; i < node.Fields.Length; i++)
                fieldNodes[node.Fields.Length - 1 - i] = Nodes.Pop();

            Expression sequence = Nodes.Pop();

            for (int i = 0; i < fieldNodes.Length; i++)
            {
                var fieldNode = node.Fields[i];
                var fieldPredicate = fieldNodes[i].ConverToComparableType();
                if (i == 0)
                {
                    if (fieldNode.Order == Order.Ascending)
                    {
                        sequence = sequence.OrderBy(Expression.Lambda(fieldPredicate, item));
                    }
                    else
                    {
                        sequence = sequence.OrderByDescending(Expression.Lambda(fieldPredicate, item));
                    }
                }
                else
                {
                    if (fieldNode.Order == Order.Ascending)
                    {
                        sequence = sequence.ThenBy(Expression.Lambda(fieldPredicate, item));
                    }
                    else
                    {
                        sequence = sequence.ThenByDescending(Expression.Lambda(fieldPredicate, item));
                    }
                }
            }

            Nodes.Push(sequence);
        }

        public void Visit(CreateTableNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(CaseNode node)
        {
            var returnLabel = Expression.Label(node.ReturnType, "return");

            // when then
            List<Expression> statements = new List<Expression>();
            for (int i = 0; i < node.WhenThenPairs.Length; i++)
            {
                Expression then = Nodes.Pop();
                Expression when = Nodes.Pop();
                statements.Add(Expression.IfThen(
                    when,
                    Expression.Return(returnLabel, then)));
            }

            // else
            Expression elseThen = Nodes.Pop();
            statements.Add(Expression.Return(returnLabel, elseThen));

            // return value
            statements.Add(Expression.Label(returnLabel, Expression.Default(node.ReturnType)));

            var caseStatement = Expression.Invoke(Expression.Lambda(Expression.Block(statements)));

            Nodes.Push(caseStatement);
        }

        public void Visit(TypeNode node)
        {
            Nodes.Push(Expression.Constant(node.ReturnType));
        }

        public void Visit(NullNode node)
        {
            Nodes.Push(Expression.Constant(null, node.ReturnType));
        }

        public void Visit(ExecuteNode node)
        {
            Expression valueExpression = Nodes.Pop();
            var valueAsObjectExpression = Expression.Convert(valueExpression, typeof(object));
            var value = Expression.Lambda<Func<object>>(valueAsObjectExpression).Compile()();
            if (node.VariableToSet != null)
            {
                _engine.SetVariable(node.VariableToSet.Name, value);
            }
        }

        public void SetScope(Scope scope)
        {
            // if (scope?.Name == "Query")
            //     _queryState.Input = null;
        }

        public void SetQueryIdentifier(string identifier)
        {

        }
    }

}