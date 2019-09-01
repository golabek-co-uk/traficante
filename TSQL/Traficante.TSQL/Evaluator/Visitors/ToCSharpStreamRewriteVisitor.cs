using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;
using Traficante.TSQL.Evaluator.Utils;
using Traficante.TSQL.Parser;
using Traficante.TSQL.Parser.Nodes;
using Traficante.TSQL.Parser.Tokens;
using Traficante.TSQL.Schema;
using Traficante.TSQL.Schema.DataSources;
using TextSpan = Traficante.TSQL.Parser.TextSpan;

namespace Traficante.TSQL.Evaluator.Visitors
{
    public class ToCSharpStreamRewriteVisitor : IExpressionVisitor
    {
        private readonly IDictionary<string, int[]> _setOperatorFieldIndexes;

        private int _setOperatorMethodIdentifier;
        private int _caseWhenMethodIndex = 0;

        private string _queryAlias;
        private Scope _scope;
        private MethodAccessType _type;

        ExpressionHelper expressionHelper = new ExpressionHelper();

        //Type _itemType = null;
        ParameterExpression _item = null;// Expression.Parameter(typeof(IObjectResolver), "inputItem");
        Dictionary<string, Expression> _alias2Item = new Dictionary<string, Expression>();

        //Type groupItemType = null;
        ParameterExpression _itemInGroup = null;

        ParameterExpression _input = null;

        Dictionary<string, Expression> _cte = new Dictionary<string, Expression>();

        Stack<System.Linq.Expressions.Expression> Nodes { get; set; }
        private IEngine _engine;
        private RuntimeContext _interCommunicator;

        public IQueryable<IObjectResolver> ResultStream = null;
        public IDictionary<string, string[]> ResultColumns = new Dictionary<string, string[]>();
        public IDictionary<string, Type[]> ResultColumnsTypes = new Dictionary<string, Type[]>();


        private IDictionary<Node, IColumn[]> InferredColumns { get; }

        public ToCSharpStreamRewriteVisitor(
            IEngine engine,
            IDictionary<string, int[]> setOperatorFieldIndexes, 
            IDictionary<Node, IColumn[]> inferredColumns)
        {
            _setOperatorFieldIndexes = setOperatorFieldIndexes;
            InferredColumns = inferredColumns;
            Nodes = new Stack<System.Linq.Expressions.Expression>();
            _engine = engine;
            _interCommunicator = RuntimeContext.Empty;

        }
        
        public void Visit(Node node)
        {
        }

        public void Visit(DescNode node)
        {
            if (node.Type == DescForType.SpecificConstructor)
            {
                var fromNode = (SchemaFunctionFromNode)node.From;

                var table = _engine
                    .GetDatabase(null)
                    .GetTableByName(fromNode.Schema, fromNode.Method);

                var descType = expressionHelper.CreateAnonymousType(new (string, Type)[3] {
                    ("Name", typeof(string)),
                    ("Index", typeof(int)),
                    ("Type", typeof(string))
                });

                var columnsType = typeof(List<>).MakeGenericType(descType);
                var columns = columnsType.GetConstructors()[0].Invoke(new object[0]);
                for (int i = 0; i < table.Columns.Length; i++)
                {
                    var descObj = descType.GetConstructors()[0].Invoke(new object[0]);
                    descType.GetField("Name").SetValue(descObj, table.Columns[i].ColumnName);
                    descType.GetField("Index").SetValue(descObj, table.Columns[i].ColumnIndex);
                    descType.GetField("Type").SetValue(descObj, table.Columns[i].ColumnType.ToString());
                    columnsType.GetMethod("Add", new Type[] { descType }).Invoke(columns, new object[] { descObj });
                }

                Nodes.Push(Expression.Constant(columns));
                ResultColumns[fromNode.Alias] = descType.GetFields().Select(x => x.Name).ToArray();
                ResultColumnsTypes[fromNode.Alias] = descType.GetFields().Select(x => x.FieldType).ToArray();
                return;
            }
            if (node.Type == DescForType.Schema)
            {
                var fromNode = (SchemaFunctionFromNode)node.From;

                var table = _engine
                    .GetDatabase(fromNode.Schema);
            }
        }

        public void Visit(StarNode node)
        {
            var b = Nodes.Pop();
            var a = Nodes.Pop();
            (a, b) = this.expressionHelper.AlignSimpleTypes(a, b);
            Nodes.Push(Expression.Multiply(a, b));
        }

        public void Visit(FSlashNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            (left, right) = this.expressionHelper.AlignSimpleTypes(left, right);
            Nodes.Push(Expression.Divide(left, right));
        }

        public void Visit(ModuloNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            (left, right) = this.expressionHelper.AlignSimpleTypes(left, right);
            Nodes.Push(Expression.Modulo(left, right));
        }

        public void Visit(AddNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            (left, right) = this.expressionHelper.AlignSimpleTypes(left, right);
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
            (left, right) = this.expressionHelper.AlignSimpleTypes(left, right);
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

        public void Visit(ShortCircuitingNodeLeft node)
        {
            throw new NotImplementedException();
        }

        public void Visit(ShortCircuitingNodeRight node)
        {
            throw new NotImplementedException();
        }

        public void Visit(EqualityNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(this.expressionHelper.SqlLikeOperation(left, right, Expression.Equal));
        }

        public void Visit(GreaterOrEqualNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(this.expressionHelper.SqlLikeOperation(left, right, Expression.GreaterThanOrEqual));
        }

        public void Visit(LessOrEqualNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(this.expressionHelper.SqlLikeOperation(left, right, Expression.LessThanOrEqual));
        }

        public void Visit(GreaterNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(this.expressionHelper.SqlLikeOperation(left, right, Expression.GreaterThan));
        }

        public void Visit(LessNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(this.expressionHelper.SqlLikeOperation(left, right, Expression.LessThan));
        }

        public void Visit(DiffNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(this.expressionHelper.SqlLikeOperation(left, right, Expression.NotEqual));
        }

        public void Visit(NotNode node)
        {
            Nodes.Push(Expression.Not(Nodes.Pop()));
        }
        public void Visit(LikeNode node)
        {
            Visit(new AccessMethodNode(
                new FunctionToken(nameof(Operators.Like), TextSpan.Empty),
                new ArgsListNode(new[] { node.Left, node.Right }), null,
                typeof(Operators).GetMethod(nameof(Operators.Like))));
        }

        public void Visit(RLikeNode node)
        {
            Visit(new AccessMethodNode(
                new FunctionToken(nameof(Operators.RLike), TextSpan.Empty),
                new ArgsListNode(new[] { node.Left, node.Right }), null,
                typeof(Operators).GetMethod(nameof(Operators.RLike))));
        }

        public void Visit(InNode node)
        {
            throw new NotImplementedException();
        }

        private FieldNode _currentFieldNode = null;
        public void SetFieldNode(FieldNode node)
        {
            _currentFieldNode = node;
        }

        public void Visit(FieldNode node)
        {
            /// TODO: add check if conversion is needed
            var value = Nodes.Pop();
            Nodes.Push(Expression.Convert(value, node.ReturnType));
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
            var rightQueryable = Expression.Call(
                typeof(Queryable),
                "AsQueryable", new Type[] { rightNode.Args[0].ReturnType }, right);

            var left = Nodes.Pop();

            MethodCallExpression containsCall = Expression.Call(
                typeof(Queryable),
                "Contains",
                new Type[] { right.Type.GetElementType() },
                rightQueryable,
                left);

            Nodes.Push(containsCall);
        }

        public void Visit(AccessMethodNode node)
        {
            var args = Enumerable.Range(0, node.ArgsCount).Select(x => Nodes.Pop()).Reverse().ToArray();
            var argsTypes = args.Select(x => x.Type).ToArray();

            if (node.IsAggregateMethod)
            {
                if (this._item.Type.Name == "IGrouping`2")
                {

                    if (node.Method.Name == "Count")
                    {
                        var selector = Expression.Lambda(args[0], this._itemInGroup);
                        var group = Expression.Convert(this._item, typeof(IEnumerable<>).MakeGenericType(this._itemInGroup.Type));
                        MethodCallExpression selectCall = Expression.Call(
                            typeof(Enumerable),
                            "Select",
                            new Type[] { this._itemInGroup.Type, node.Arguments.Args[0].ReturnType },
                            new Expression[] { group, selector });
                        MethodCallExpression call = Expression.Call(
                            typeof(Enumerable),
                            "Count",
                            new Type[] { node.Arguments.Args[0].ReturnType },
                            new Expression[] { selectCall });
                        Nodes.Push(call);
                        return;
                    }
                    if (node.Method.Name == "Sum")
                    {
                        var selector =  Expression.Lambda(args[0], this._itemInGroup);
                        var group = Expression.Convert(this._item, typeof(IEnumerable<>).MakeGenericType(this._itemInGroup.Type));
                        MethodCallExpression selectCall = Expression.Call(
                            typeof(Enumerable),
                            "Select",
                            new Type[] { this._itemInGroup.Type, node.Arguments.Args[0].ReturnType },
                            new Expression[] { group, selector });
                        MethodCallExpression call = Expression.Call(
                            typeof(Enumerable),
                            "Sum",
                            new Type[] { },
                            new Expression[] { selectCall });
                        Nodes.Push(call);
                        return;
                    }
                    if (node.Method.Name == "Max")
                    {
                        var selector = Expression.Lambda(args[0], this._itemInGroup);
                        var group = Expression.Convert(this._item, typeof(IEnumerable<>).MakeGenericType(this._itemInGroup.Type));
                        MethodCallExpression selectCall = Expression.Call(
                            typeof(Enumerable),
                            "Select",
                            new Type[] { this._itemInGroup.Type, node.Arguments.Args[0].ReturnType },
                            new Expression[] { group, selector });
                        MethodCallExpression call = Expression.Call(
                            typeof(Enumerable),
                            "Max",
                            new Type[] { },
                            new Expression[] { selectCall });
                        Nodes.Push(call);
                        return;
                    }
                    if (node.Method.Name == "Min")
                    {
                        var selector = Expression.Lambda(args[0], this._itemInGroup);
                        var group = Expression.Convert(this._item, typeof(IEnumerable<>).MakeGenericType(this._itemInGroup.Type));
                        MethodCallExpression selectCall = Expression.Call(
                            typeof(Enumerable),
                            "Select",
                            new Type[] { this._itemInGroup.Type, node.Arguments.Args[0].ReturnType },
                            new Expression[] { group, selector });
                        MethodCallExpression call = Expression.Call(
                            typeof(Enumerable),
                            "Min",
                            new Type[] { },
                            new Expression[] { selectCall });
                        Nodes.Push(call);
                        return;
                    }
                    if (node.Method.Name == "Avg")
                    {
                        var selector = Expression.Lambda(args[0], this._itemInGroup);
                        var group = Expression.Convert(this._item, typeof(IEnumerable<>).MakeGenericType(this._itemInGroup.Type));
                        MethodCallExpression selectCall = Expression.Call(
                            typeof(Enumerable),
                            "Select",
                            new Type[] { this._itemInGroup.Type, node.Arguments.Args[0].ReturnType },
                            new Expression[] { group, selector });
                        MethodCallExpression call = Expression.Call(
                            typeof(Enumerable),
                            "Average",
                            new Type[] { },
                            new Expression[] { selectCall });
                        Nodes.Push(call);
                        return;
                    }
                    throw new ApplicationException($"Aggregate method  {node.Method.Name} is not supported.");
                }
                else
                {
                    if (node.Method.Name == "Count")
                    {
                        var selector = Expression.Lambda(args[0], this._item);
                        MethodCallExpression selectCall = Expression.Call(
                            typeof(Queryable),
                            "Select",
                            new Type[] { this._item.Type, node.Arguments.Args[0].ReturnType },
                            new Expression[] { this._input, selector });
                        MethodCallExpression call = Expression.Call(
                            typeof(Queryable),
                            "Count",
                            new Type[] { node.Arguments.Args[0].ReturnType },
                            new Expression[] { selectCall });
                        Nodes.Push(call);
                        return;
                    }
                    if (node.Method.Name == "Sum")
                    {
                        var selector = Expression.Lambda(args[0], this._item);
                        MethodCallExpression selectCall = Expression.Call(
                            typeof(Queryable),
                            "Select",
                            new Type[] { this._item.Type, node.Arguments.Args[0].ReturnType },
                            new Expression[] { this._input, selector });
                        MethodCallExpression call = Expression.Call(
                            typeof(Queryable),
                            "Sum",
                            new Type[] {},
                            new Expression[] { selectCall });
                        Nodes.Push(call);
                        return;
                    }
                    if (node.Method.Name == "Avg")
                    {
                        var selector = Expression.Lambda(args[0], this._item);
                        MethodCallExpression selectCall = Expression.Call(
                            typeof(Queryable),
                            "Select",
                            new Type[] { this._item.Type, node.Arguments.Args[0].ReturnType },
                            new Expression[] { this._input, selector });
                        MethodCallExpression call = Expression.Call(
                            typeof(Queryable),
                            "Average",
                            new Type[] { },
                            new Expression[] { selectCall });
                        Nodes.Push(call);
                        return;
                    }
                    if (node.Method.Name == "Max")
                    {
                        var selector = Expression.Lambda(args[0], this._item);
                        MethodCallExpression selectCall = Expression.Call(
                            typeof(Queryable),
                            "Select",
                            new Type[] { this._item.Type, node.Arguments.Args[0].ReturnType },
                            new Expression[] { this._input, selector });
                        MethodCallExpression call = Expression.Call(
                            typeof(Queryable),
                            "Max",
                            new Type[] { },
                            new Expression[] { selectCall });
                        Nodes.Push(call);
                        return;
                    }
                    if (node.Method.Name == "Min")
                    {
                        var selector = Expression.Lambda(args[0], this._item);
                        MethodCallExpression selectCall = Expression.Call(
                            typeof(Queryable),
                            "Select",
                            new Type[] { this._item.Type, node.Arguments.Args[0].ReturnType },
                            new Expression[] { this._input, selector });
                        MethodCallExpression call = Expression.Call(
                            typeof(Queryable),
                            "Min",
                            new Type[] { },
                            new Expression[] { selectCall });
                        Nodes.Push(call);
                        return;
                    }
                    throw new ApplicationException($"Aggregate method  {node.Method.Name} is not supported.");
                }
            }
            else
            {
                if (this._item?.Type.Name == "IGrouping`2")
                {
                    var key = Expression.PropertyOrField(this._item, "Key");
                    if (key.Type.GetFields().Any(x => string.Equals(x.Name, this._currentFieldNode.FieldName)))
                    {
                        Nodes.Push(Expression.PropertyOrField(key, this._currentFieldNode.FieldName));
                        return;
                    }
                }

                var instance = node.Method.ReflectedType.GetConstructors()[0].Invoke(new object[] { });
                /// TODO: check if there can be more that one generic argument
                var method = node.Method.IsGenericMethodDefinition ?
                    node.Method.MakeGenericMethod(node.ReturnType) : node.Method;

                var parameters = method.GetParameters();
                for(int i = 0; i < parameters.Length; i++)
                {
                    args[i] = this.expressionHelper.AlignSimpleTypes(args[i], parameters[i].ParameterType);
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

        public void Visit(AccessRawIdentifierNode node)
        {
            throw new NotImplementedException();
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

        //public void Visit(AccessRefreshAggreationScoreNode node)
        //{
        //    throw new NotImplementedException();
        //}

        public void Visit(AccessColumnNode node)
        {
            if (_item.Type.Name == "IGrouping`2")
            {
                // TODO: just for testing. 
                // come with idea, how to figure out if the colum is inside aggregation function 
                // or is just column to display
                //try
                //{
                var fieldNameameWithAlias = node.Alias + "." + node.Name;

                var key = Expression.PropertyOrField(this._item, "Key");
                if (key.Type.GetFields().Any(x => string.Equals(x.Name, fieldNameameWithAlias)))
                {
                    var properyOfKey = Expression.PropertyOrField(key, fieldNameameWithAlias);
                    Nodes.Push(properyOfKey);
                    return;
                } else
                if (key.Type.GetFields().Any(x => string.Equals(x.Name, node.Name)))
                {
                    var properyOfKey = Expression.PropertyOrField(key, node.Name);
                    Nodes.Push(properyOfKey);
                    return;
                }
                else
                {
                    var aliasProperty = this._itemInGroup.Type.GetFields().FirstOrDefault(x => string.Equals(x.Name, node.Alias));
                    if (aliasProperty != null)
                    {
                        var nameProperty = aliasProperty.FieldType.GetFields().FirstOrDefault(x => string.Equals(x.Name, node.Name));
                        if (nameProperty != null)
                        {
                            Nodes.Push(
                                Expression.PropertyOrField( 
                                    Expression.PropertyOrField(this._itemInGroup, node.Alias),
                                    node.Name));
                            return;
                        }
                    }
                    var groupItemProperty = Expression.PropertyOrField(this._itemInGroup, node.Name);
                    Nodes.Push(groupItemProperty);
                    return;
                }
                //}
                //catch (Exception ex)
                //{
                //    var groupItemProperty = Expression.PropertyOrField(this._itemInGroup, node.Name);
                //    Nodes.Push(groupItemProperty);
                //}
            }

            var item = _alias2Item[node.Alias];
            var property = Expression.PropertyOrField(item, node.Name);
            Nodes.Push(property);
        }

        public void Visit(AllColumnsNode node)
        {
        }

        public void Visit(IdentifierNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(AccessObjectArrayNode node)
        {
            var property = Nodes.Pop();
            var array = Expression.PropertyOrField(property, node.ObjectName);
            var index = Expression.Constant(node.Token.Index);
            Nodes.Push(Expression.ArrayAccess(array, index));
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
            _engine.SetVariable(node.Variable.Name, node.Type.ReturnType, null);
        }

        public void Visit(SetNode node)
        {
            Expression valueExpression = Nodes.Pop();
            var value = Expression.Lambda<Func<object>>(valueExpression).Compile()();
            _engine.SetVariable(node.Variable.Name, value);
        }

        public void Visit(DotNode node)
        {
        }

        public void Visit(AccessCallChainNode node)
        {
        }

        public void Visit(ArgsListNode node)
        {
            //Nodes.Push(node);
        }

        public void Visit(SelectNode node)
        {
            var outputFields = new (FieldNode Field, Expression Value)[node.Fields.Length];
            for (var i = 0; i < node.Fields.Length; i++)
                outputFields[node.Fields.Length - 1 - i] = (node.Fields[node.Fields.Length - 1 - i], Nodes.Pop());

            var outputItemType = expressionHelper.CreateAnonymousType(outputFields.Select(x => (x.Field.FieldName, x.Field.ReturnType)));

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

            if (node.ReturnsSingleRow.HasValue && node.ReturnsSingleRow.Value)
            {
                var array = Expression.NewArrayInit(outputItemType, new Expression[] { initialization });

                var call = Expression.Call(
                    typeof(Queryable),
                    "AsQueryable",
                    new Type[] { outputItemType },
                    array);

                if (_input != null)
                    Nodes.Push(Expression.Lambda(call, _input));
                else
                    Nodes.Push(Expression.Lambda(call));
            }
            else
            {
                //"item => new AnonymousType() { SelectProp = item.name, SelectProp2 = item.SelectProp2) }"
                Expression expression = Expression.Lambda(initialization, _item);

                var call = Expression.Call(
                    typeof(Queryable),
                    "Select",
                    new Type[] { this._item.Type, outputItemType },
                    _input,
                    expression);

                Nodes.Push(Expression.Lambda(call, _input));
            }


            //"AnonymousType input"
            this._item = Expression.Parameter(outputItemType, "item_" + outputItemType.Name);

            //"IQueryable<AnonymousType> input"
            this._input = Expression.Parameter(typeof(IQueryable<>).MakeGenericType(outputItemType), "input");
        }

        public void Visit(GroupSelectNode node)
        {
        }

        public void Visit(WhereNode node)
        {
            var predicate =  Nodes.Pop();
            var predicateLambda = Expression.Lambda(predicate, this._item);

            MethodCallExpression call = Expression.Call(
                typeof(Queryable),
                "Where",
                new Type[] { this._item.Type },
                _input,
                predicateLambda);

            Nodes.Push(Expression.Lambda(call, _input));
        }

        public void Visit(GroupByNode node)
        {
            var outputFields = new (FieldNode Field, Expression Value)[node.Fields.Length];
            for (var i = 0; i < node.Fields.Length; i++)
                outputFields[node.Fields.Length - 1 - i] = (node.Fields[node.Fields.Length - 1 - i], Nodes.Pop());
            var outputItemType = expressionHelper.CreateAnonymousType(outputFields.Select(x => (x.Field.FieldName, x.Field.ReturnType)));

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
            Expression expression = Expression.Lambda(initialization, _item);

            var groupByCall = Expression.Call(
                typeof(Queryable),
                "GroupBy",
                new Type[] { this._item.Type, outputItemType },
                _input,
                expression);

            Nodes.Push(Expression.Lambda(groupByCall, _input));


            // "ItemAnonymousType itemInGroup "
            this._itemInGroup = Expression.Parameter(this._item.Type, "itemInGroup_" + this._item.Type);

            // "IGrouping<KeyAnonymousType, ItemAnonymousType>"
            outputItemType = typeof(IGrouping<,>).MakeGenericType(outputItemType, this._item.Type);

            // "IGrouping<KeyAnonymousType, ItemAnonymousType> item"
            this._item = Expression.Parameter(outputItemType, "item_" + outputItemType.Name);

            // "IQueryable<IGrouping<KeyAnonymousType, ItemAnonymousType>> input"
            this._input = Expression.Parameter(typeof(IQueryable<>).MakeGenericType(outputItemType), "input");
        }

        public void Visit(HavingNode node)
        {
            var predicate = Nodes.Pop();
            var predicateLambda = Expression.Lambda(predicate, this._item);

            MethodCallExpression call = Expression.Call(
                typeof(Queryable),
                "Where",
                new Type[] { this._item.Type },
                _input,
                predicateLambda);

            Nodes.Push(Expression.Lambda(call, _input));
        }        

        public void Visit(SkipNode node)
        {
            //"IQueryable<AnonymousType> input"
            this._input = Expression.Parameter(typeof(IQueryable<>).MakeGenericType(this._item.Type), "input");
            

            var skipNumber = Expression.Constant((int)node.Value);

            MethodCallExpression call = Expression.Call(
                typeof(Queryable),
                "Skip",
                new Type[] { this._item.Type },
                _input,
                skipNumber);

            Nodes.Push(Expression.Lambda(call, _input));
        }

        public void Visit(TakeNode node)
        {
            var takeNumber = Expression.Constant((int)node.Value);

            MethodCallExpression call = Expression.Call(
                typeof(Queryable),
                "Take",
                new Type[] { this._item.Type },
                _input,
                takeNumber);

            Nodes.Push(Expression.Lambda(call, _input));
        }

        public void Visit(JoinInMemoryWithSourceTableFromNode node)
        {
            throw new NotImplementedException();
        }

        
        public void Visit(SchemaFunctionFromNode node)
        {
            //var rowSource = _schemaProvider.GetDatabase(null).GetRowSource(node.Schema, node.Method, _interCommunicator, new object[0]).Rows;
            var rowSource = _engine.GetDatabase(null).GetFunctionRowSource(node.Schema, node.Method, new object[0]).Rows;

            var fields = _engine
                .GetDatabase(null)
                .GetFunctionByName(node.Schema, node.Method, new object[0])
                .Columns.Select(x => (x.ColumnName, x.ColumnType)).ToArray();

            Type entityType = expressionHelper.CreateAnonymousType(fields);

            var rowOfDataSource = Expression.Parameter(typeof(IObjectResolver), "rowOfDataSource");

            List<MemberBinding> bindings = new List<MemberBinding>();
            foreach (var field in fields)
            {
                //"SelectProp = rowOfDataSource.GetValue(..fieldName..)"
                MemberBinding assignment = Expression.Bind(
                    entityType.GetField(field.Item1), 
                    Expression.Convert(Expression.Call(rowOfDataSource, "GetValue", new Type[] {}, new[] { Expression.Constant(field.Item1)}), field.Item2));
                bindings.Add(assignment);
            }

            //"new AnonymousType()"
            var creationExpression = Expression.New(entityType.GetConstructor(Type.EmptyTypes));

            //"new AnonymousType() { SelectProp = item.name, SelectProp2 = item.SelectProp2) }"
            var initialization = Expression.MemberInit(creationExpression, bindings);

            //"item => new AnonymousType() { SelectProp = item.name, SelectProp2 = item.SelectProp2) }"
            Expression expression = Expression.Lambda(initialization, rowOfDataSource);

            var queryableRowSource = Expression.Constant(rowSource.AsQueryable());

            var call = Expression.Call(
                typeof(Queryable),
                "Select",
                new Type[] { typeof(IObjectResolver), entityType },
                queryableRowSource,
                expression);

            Nodes.Push(call);

            //"AnonymousType input"
            this._item = Expression.Parameter(entityType, "item_" + entityType.Name);
            this._alias2Item[node.Alias] = this._item;

            //"IQueryable<AnonymousType> input"
            this._input = Expression.Parameter(typeof(IQueryable<>).MakeGenericType(entityType), "input");

            ResultColumns[node.Alias] = fields.Select(x => x.Item1).ToArray();
            ResultColumnsTypes[node.Alias] = fields.Select(x => x.Item2).ToArray();
        }

        public void Visit(SchemaTableFromNode node)
        {
            var rowSource = _engine.GetDatabase(node.Database).GetTableRowSource(node.Schema, node.TableOrView).Rows;

            var fields = _engine
                .GetDatabase(node.Database)
                .GetTableByName(node.Schema, node.TableOrView)
                .Columns.Select(x => (x.ColumnName, x.ColumnType)).ToArray();

            Type entityType = expressionHelper.CreateAnonymousType(fields);

            var rowOfDataSource = Expression.Parameter(typeof(IObjectResolver), "rowOfDataSource");

            List<MemberBinding> bindings = new List<MemberBinding>();
            foreach (var field in fields)
            {
                //"SelectProp = rowOfDataSource.GetValue(..fieldName..)"
                MemberBinding assignment = Expression.Bind(
                    entityType.GetField(field.Item1),
                    Expression.Convert(Expression.Call(rowOfDataSource, "GetValue", new Type[] { }, new[] { Expression.Constant(field.Item1) }), field.Item2));
                bindings.Add(assignment);
            }

            //"new AnonymousType()"
            var creationExpression = Expression.New(entityType.GetConstructor(Type.EmptyTypes));

            //"new AnonymousType() { SelectProp = item.name, SelectProp2 = item.SelectProp2) }"
            var initialization = Expression.MemberInit(creationExpression, bindings);

            //"item => new AnonymousType() { SelectProp = item.name, SelectProp2 = item.SelectProp2) }"
            Expression expression = Expression.Lambda(initialization, rowOfDataSource);

            var queryableRowSource = Expression.Constant(rowSource.AsQueryable());

            var call = Expression.Call(
                typeof(Queryable),
                "Select",
                new Type[] { typeof(IObjectResolver), entityType },
                queryableRowSource,
                expression);

            Nodes.Push(call);

            //"AnonymousType input"
            this._item = Expression.Parameter(entityType, "item_" + entityType.Name);
            this._alias2Item[node.Alias] = this._item;

            //"IQueryable<AnonymousType> input"
            this._input = Expression.Parameter(typeof(IQueryable<>).MakeGenericType(entityType), "input");

            ResultColumns[node.Alias] = fields.Select(x => x.Item1).ToArray();
            ResultColumnsTypes[node.Alias] = fields.Select(x => x.Item2).ToArray();
        }

        public void Visit(AliasedFromNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(JoinSourcesTableFromNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(InMemoryTableFromNode node)
        {
            var table = _cte[node.VariableName];

            //Get from IQueryable<AnonymousType>
            var outputitemType = table.Type.GetGenericArguments()[0]; 
           
            //"AnonymousType input"
            this._item = Expression.Parameter(outputitemType, "item_" + outputitemType.Name);
            this._alias2Item[node.Alias] = this._item;

            //"IQueryable<AnonymousType> input"
            this._input = Expression.Parameter(typeof(IQueryable<>).MakeGenericType(outputitemType), "input");

            Nodes.Push(table);
        }

        public void Visit(ReferentialFromNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(JoinFromNode node)
        {
        }

        public void Visit(ExpressionFromNode node)
        {
        }

        public void Visit(SchemaMethodFromNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(CreateTransformationTableNode node)
        {
        }

        //public void Visit(RenameTableNode node)
        //{
        //}

        public void Visit(TranslatedSetTreeNode node)
        {
        }

        public void Visit(IntoNode node)
        {
        }

        public void Visit(QueryScope node)
        {
        }

        public void Visit(ShouldBePresentInTheTable node)
        {
        }

        public void Visit(TranslatedSetOperatorNode node)
        {
        }

        public void Visit(QueryNode node)
        {
            Expression select = node.Select != null ? Nodes.Pop() : null;

            Expression orderBy = node.OrderBy != null ? Nodes.Pop() : null;

            Expression take = node.Take != null ? Nodes.Pop() : null;
            Expression skip = node.Skip != null ? Nodes.Pop() : null;

            Expression having = (node.GroupBy != null && node.GroupBy.Having != null) ? Nodes.Pop() : null;

            Expression groupBy = node.GroupBy != null ? Nodes.Pop() : null;

            Expression where = node.Where != null ? Nodes.Pop() : null;

            Expression from = node.From != null ? Nodes.Pop() : null;


            Expression last = from;

            if (where != null)
            {
                last = Expression.Invoke(where, last);
            }

            if (groupBy != null)
            {
                last = Expression.Invoke(groupBy, last);
            }

            if (having != null)
            {
                last = Expression.Invoke(having, last);
            }

            if (skip != null)
            {
                last = Expression.Invoke(skip, last);
            }

            if (take != null)
            {
                last = Expression.Invoke(take, last);
            }

            if (orderBy != null)
            {
                last = Expression.Invoke(orderBy, last);
            }

            if (select != null)
            {
                if (last != null)
                {
                    last = Expression.Invoke(select, last);
                }
                else
                {
                    last = Expression.Invoke(select);
                }
            }

            Nodes.Push(last);
            ResultColumns[_queryAlias ?? ""] = node.Select.Fields.Select(x => x.FieldName).ToArray();
            ResultColumnsTypes[_queryAlias ?? ""] = node.Select.Fields.Select(x => x.ReturnType).ToArray();
        }

        public void Visit(InternalQueryNode node)
        {
            Expression skip = node.Skip != null ? Nodes.Pop() : null;
            Expression take = node.Take != null ? Nodes.Pop() : null;

            Expression select = node.Select != null ? Nodes.Pop() : null;
            Expression where = node.Where != null ? Nodes.Pop() : null;

            Expression groupBy = node.GroupBy != null ? Nodes.Pop() : null;

            Expression from = node.From != null ? Nodes.Pop() : null;


            Expression last = from;

            if (where != null)
            {
                last = Expression.Invoke(where, last);
            }

            if (skip != null)
            {
                last = Expression.Invoke(skip, last);
            }

            if (take != null)
            {
                last = Expression.Invoke(take, last);
            }

            if (groupBy != null)
            {
                last = Expression.Invoke(groupBy, last);
            }

            if (select != null)
            {
                last = Expression.Invoke(select, last);
            }
            Nodes.Push(last);
        }

        public void Visit(RootNode node)
        {
            if (Nodes.Any())
            {
                Expression last = Nodes.Pop();
                Expression<Func<IEnumerable<object>>> toStream = Expression.Lambda<Func<IEnumerable<object>>>(last);
                var compiledToStream = toStream.Compile();
                ResultStream = compiledToStream().AsQueryable().Select(x => new AnonymousTypeResolver(x));
            }
        }

        public void Visit(SingleSetNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(UnionNode node)
        {
            var right = Nodes.Pop();
            var rightType = this.expressionHelper.GetQueryableItemType(right);

            var left = Nodes.Pop();
            var leftType = this.expressionHelper.GetQueryableItemType(left);

            var outputItemType = expressionHelper.CreateAnonymousTypeSameAs(leftType);
            var leftSelect = this.expressionHelper.Select(left, outputItemType);
            var rightSelect = this.expressionHelper.Select(right, outputItemType);

            List<Expression> prameters = new List<Expression> { leftSelect, rightSelect };
            if (node.Keys.Length > 0)
            {
                var comparerType = expressionHelper.CreateEqualityComparerForType(outputItemType, node.Keys);
                var comparer = Expression.New(comparerType);
                prameters.Add(comparer);
            }

            var call = Expression.Call(
                typeof(Queryable),
                "Union",
                new Type[] { outputItemType },
                prameters.ToArray());

            Nodes.Push(call);
            
        }

        public void Visit(UnionAllNode node)
        {
            var right = Nodes.Pop();
            var rightType = this.expressionHelper.GetQueryableItemType(right);

            var left = Nodes.Pop();
            var leftType = this.expressionHelper.GetQueryableItemType(left);

            var outputItemType = expressionHelper.CreateAnonymousTypeSameAs(leftType);
            var leftSelect = this.expressionHelper.Select(left, outputItemType);
            var rightSelect = this.expressionHelper.Select(right, outputItemType);

            var call = Expression.Call(
                typeof(Queryable),
                "Concat",
                new Type[] { outputItemType },
                leftSelect,
                rightSelect);

            Nodes.Push(call);
        }

        public void Visit(ExceptNode node)
        {
            var right = Nodes.Pop();
            var rightType = this.expressionHelper.GetQueryableItemType(right);

            var left = Nodes.Pop();
            var leftType = this.expressionHelper.GetQueryableItemType(left);
            
            var outputItemType = expressionHelper.CreateAnonymousTypeSameAs(leftType);
            var leftSelect = this.expressionHelper.Select(left, outputItemType);
            var rightSelect = this.expressionHelper.Select(right, outputItemType);

            List<Expression> prameters = new List<Expression> { leftSelect , rightSelect };
            if (node.Keys.Length > 0)
            {
                var comparerType = expressionHelper.CreateEqualityComparerForType(outputItemType, node.Keys);
                var comparer = Expression.New(comparerType);
                prameters.Add(comparer);
            }

            var call = Expression.Call(
                typeof(Queryable),
                "Except",
                new Type[] { outputItemType },
                prameters.ToArray());

            Nodes.Push(call);
        }

        //public void Visit(RefreshNode node)
        //{
        //}

        public void Visit(IntersectNode node)
        {
            var right = Nodes.Pop();
            var rightType = right.Type.GetGenericArguments()[0]; //IQueryable<AnonymousType>

            var left = Nodes.Pop();
            var leftType = left.Type.GetGenericArguments()[0]; //IQueryable<AnonymousType>

            var outputItemType = expressionHelper.CreateAnonymousTypeSameAs(leftType);
            var leftSelect = this.expressionHelper.Select(left, outputItemType);
            var rightSelect = this.expressionHelper.Select(right, outputItemType);

            var call = Expression.Call(
                typeof(Queryable),
                "Intersect",
                new Type[] { outputItemType },
                leftSelect,
                rightSelect);

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
            throw new NotImplementedException();
        }

        public void Visit(StatementNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(CteExpressionNode node)
        {
        }

        public void Visit(CteInnerExpressionNode node)
        {
            _cte[node.Name] = Nodes.Pop();
        }

        public void Visit(JoinsNode node)
        {
            var joinNodes = new List<(
                JoinFromNode JoinNode, 
                Expression OnExpression, 
                Expression JoinExpression,
                Type ItemType,
                string ItemAlias)>();
            FromNode fromNode = null;
            Expression fromExpression = null;
            Type fromItemType = null;
            string fromItemAlias = null;
            JoinFromNode joinNode = node.Joins;
            do
            {
                var onExpression = Nodes.Pop();
                var joinExpression = Nodes.Pop();
                var itemType = this.expressionHelper.GetQueryableItemType(joinExpression);
                var itemAlias = joinNode.With.Alias;
                joinNodes.Add((joinNode, onExpression, joinExpression, itemType, itemAlias));
                if (joinNode.Source is JoinFromNode)
                {
                    joinNode = joinNode.Source as JoinFromNode;
                }
                else
                {
                    fromNode = joinNode.Source;
                    fromExpression = Nodes.Pop();
                    fromItemType = this.expressionHelper.GetQueryableItemType(fromExpression);
                    fromItemAlias = fromNode.Alias;
                    joinNode = null;
                }
            } while (joinNode != null);


            var ouputTypeFields = new List<(string Alias, Type Type)>();
            foreach (var join in joinNodes)
                ouputTypeFields.Add((join.ItemAlias, join.ItemType));
            ouputTypeFields.Add((fromItemAlias, fromItemType));

            var outputItemType = this.expressionHelper.CreateAnonymousType(ouputTypeFields.ToArray());

            List<MemberBinding> bindings = new List<MemberBinding>();
            //"SelectProp = inputItem.Prop"
            foreach(var field in ouputTypeFields)
            {
                bindings.Add(Expression.Bind(
                    outputItemType.GetField(field.Alias),
                    this._alias2Item[field.Alias]));
            }
            
            //"new AnonymousType()"
            var creationExpression = Expression.New(outputItemType.GetConstructor(Type.EmptyTypes));

            //"new AnonymousType() { SelectProp = item.name, SelectProp2 = item.SelectProp2) }"
            var initialization = Expression.MemberInit(creationExpression, bindings);

            //"item => new AnonymousType() { SelectProp = item.name, SelectProp2 = item.SelectProp2) }"
            Expression expression = Expression.Lambda(initialization, (ParameterExpression)this._alias2Item[ouputTypeFields.FirstOrDefault().Alias]);

            Expression lastJoinExpression = null;
            Type lastJoinItemType = null;
            string LastJoinItemAlias = null;
            for (int i = 0; i < joinNodes.Count; i++)
            {
                var join = joinNodes[i];
                if (i == 0)
                {
                    var onCall = Expression.Call(
                        typeof(Queryable),
                        "Where",
                        new Type[] { join.ItemType },
                        join.JoinExpression,
                        Expression.Lambda(join.OnExpression, (ParameterExpression)this._alias2Item[join.ItemAlias]));

                    lastJoinExpression = Expression.Call(
                        typeof(Queryable),
                        "Select",
                        new Type[] { join.ItemType, outputItemType },
                        onCall,
                        expression);
                    lastJoinItemType = join.ItemType;
                    LastJoinItemAlias = join.ItemAlias;
                }
                else
                {
                    var onCall = Expression.Call(
                        typeof(Queryable),
                        "Where",
                        new Type[] { join.ItemType },
                        join.JoinExpression,
                        Expression.Lambda(join.OnExpression, (ParameterExpression)this._alias2Item[join.ItemAlias]));
                    var selectLambda = Expression.Lambda(
                        Expression.Convert(lastJoinExpression, typeof(IEnumerable<>).MakeGenericType(outputItemType)),
                        (ParameterExpression)this._alias2Item[join.ItemAlias]);
                    lastJoinExpression =Expression.Call(
                        typeof(Queryable),
                        "SelectMany",
                        new Type[] { join.ItemType, outputItemType },
                        onCall,
                        selectLambda);
                }
            }


            var fromLambda = Expression.Lambda(
                    Expression.Convert(lastJoinExpression, typeof(IEnumerable<>).MakeGenericType(outputItemType)),
                    (ParameterExpression)this._alias2Item[fromNode.Alias]);
            var fromCall = Expression.Call(
                typeof(Queryable),
                "SelectMany",
                new Type[] { fromItemType, outputItemType },
                fromExpression,
                fromLambda
                );

            Nodes.Push(fromCall);

            //"AnonymousType input"
            this._item = Expression.Parameter(outputItemType, "item_" + outputItemType.Name);
            this._alias2Item[node.Alias] = this._item;

            foreach (var join in joinNodes)
                this._alias2Item[join.ItemAlias] = Expression.PropertyOrField(this._item, join.ItemAlias);
            this._alias2Item[fromItemAlias] = Expression.PropertyOrField(this._item, fromItemAlias);


            this._input = Expression.Parameter(typeof(IQueryable<>).MakeGenericType(outputItemType), "input");
        }



        public void Visit(JoinNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(OrderByNode node)
        {
            throw new NotImplementedException();

            //ParameterExpression source = Expression.Parameter(typeof(IEnumerable<IObjectResolver>), "stream");
            //var queryableSource = Expression.Call(typeof(Queryable), "AsQueryable", new Type[] { typeof(IObjectResolver) }, source);


            //var call = Expression.Call(
            //    typeof(Queryable),
            //    "OrderByDescending",
            //    new Type[] { typeof(IObjectResolver), typeof(IObjectResolver) },
            //    queryableSource,
            //    selectLambda);

            //Nodes.Push(Expression.Lambda(call, source));


            //MethodCallExpression orderByCallExpression = Expression.Call(
            //    typeof(Queryable),
            //    "OrderBy",
            //    new Type[] { queryableData.ElementType, queryableData.ElementType },
            //    whereCallExpression,
            //    Expression.Lambda<Func<string, string>>(pe, new ParameterExpression[] { pe }));
        }

        public void Visit(CreateTableNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(CoupleNode node)
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

            //ParameterExpression resultResult = Expression.Parameter(node.ReturnType, "result");
            //List<(Expression Then, Expression When)> whenThenPairs = new List<(Expression Then, Expression When)>();
            //for (int i = 0; i < node.WhenThenPairs.Length; i++)
            //{
            //    (Expression Then, Expression When) whenThenPair = (Nodes.Pop(), Nodes.Pop());
            //    whenThenPairs.Add(whenThenPair);
            //}
            //Expression elseThen = Nodes.Pop();

            //Expression last = elseThen;
            //for (int i = whenThenPairs.Count - 1; i >= 0; i -= 1)
            //{
            //    last = Expression.IfThenElse(
            //        whenThenPairs[i].When,
            //        Expression.Return(returnLabel, whenThenPairs[i].Then),
            //        Expression.Return(returnLabel, last));
            //}

            //Nodes.Push(Expression.Block(last, Expression.Constant(1)));
        }

        public void SetScope(Scope scope)
        {
            _scope = scope;
        }

        public void SetQueryIdentifier(string identifier)
        {
            _queryAlias = identifier;
        }

        public void SetMethodAccessType(MethodAccessType type)
        {
            _type = type;
        }

        public void IncrementMethodIdentifier()
        {
            _setOperatorMethodIdentifier += 1;
        }

        public void Visit(TypeNode node)
        {
            Nodes.Push(Expression.Constant(node.ReturnType));
        }
    }

}