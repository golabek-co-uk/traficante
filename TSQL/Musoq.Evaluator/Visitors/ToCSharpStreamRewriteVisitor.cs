using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Musoq.Evaluator.Helpers;
using Musoq.Evaluator.Resources;
using Musoq.Evaluator.Runtime;
using Musoq.Evaluator.Tables;
using Musoq.Evaluator.Utils;
using Musoq.Evaluator.Utils.Symbols;
using Musoq.Parser;
using Musoq.Parser.Nodes;
using Musoq.Parser.Tokens;
using Musoq.Plugins;
using Musoq.Plugins.Attributes;
using Musoq.Schema;
using Musoq.Schema.DataSources;
using Musoq.Schema.Helpers;
using TextSpan = Musoq.Parser.TextSpan;

namespace Musoq.Evaluator.Visitors
{
    public class ToCSharpStreamRewriteVisitor : IExpressionVisitor
    {
        private readonly Dictionary<string, int> _inMemoryTableIndexes = new Dictionary<string, int>();
        private readonly List<string> _loadedAssemblies = new List<string>();

        private readonly List<SyntaxNode> _members = new List<SyntaxNode>();
        private readonly Stack<string> _methodNames = new Stack<string>();

        private readonly List<string> _namespaces = new List<string>();
        private readonly IDictionary<string, int[]> _setOperatorFieldIndexes;

        private readonly Dictionary<string, Type> _typesToInstantiate = new Dictionary<string, Type>();
        private BlockSyntax _emptyBlock;
        private SyntaxNode _groupHaving;

        private VariableDeclarationSyntax _groupKeys;
        private VariableDeclarationSyntax _groupValues;

        private int _inMemoryTableIndex;
        private int _setOperatorMethodIdentifier;
        private int _caseWhenMethodIndex = 0;

        private BlockSyntax _joinBlock;
        private string _queryAlias;
        private Scope _scope;
        private BlockSyntax _selectBlock;
        private MethodAccessType _type;

        ExpressionHelper expressionHelper = new ExpressionHelper();

        Type itemType = null;
        ParameterExpression item = null;// Expression.Parameter(typeof(IObjectResolver), "inputItem");

        Type groupItemType = null;
        ParameterExpression groupItem = null;

        ParameterExpression input = null;

        //ParameterExpression inputItemGroup = null;
        //ParameterExpression groupExpression = Expression.Parameter(typeof(IGrouping<IObjectResolver, IObjectResolver>), "group");

        ParameterExpression streamExpression = Expression.Parameter(typeof(IQueryable<IObjectResolver>), "stream");

        Stack<System.Linq.Expressions.Expression> Nodes { get; set; }
        private ISchemaProvider _schemaProvider;
        private RuntimeContext _interCommunicator;

        //public IDictionary<string, IQueryable<IObjectResolver>> Streams = new Dictionary<string, IQueryable<IObjectResolver>>();
        //public Stack<Expression> Streams = new Stack<Expression>();
        public IQueryable<IObjectResolver> Stream = null;
        public IDictionary<string, string[]> Columns = new Dictionary<string, string[]>();
        public IDictionary<string, Type[]> ColumnsTypes = new Dictionary<string, Type[]>();


        private IDictionary<SchemaFromNode, ISchemaColumn[]> InferredColumns { get; }

        public ToCSharpStreamRewriteVisitor(
            ISchemaProvider schemaProvider,
            IDictionary<string, int[]> setOperatorFieldIndexes, 
            IDictionary<SchemaFromNode, ISchemaColumn[]> inferredColumns)
        {
            _setOperatorFieldIndexes = setOperatorFieldIndexes;
            InferredColumns = inferredColumns;
            Nodes = new Stack<System.Linq.Expressions.Expression>();
            _schemaProvider = schemaProvider;
            _interCommunicator = RuntimeContext.Empty;

        }
        
        public void Visit(Node node)
        {
            //throw new NotImplementedException();
        }

        public void Visit(DescNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(StarNode node)
        {
            var b = Nodes.Pop();
            var a = Nodes.Pop();
            Nodes.Push(Expression.Multiply(a, b));
        }

        public void Visit(FSlashNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(Expression.Divide(left, right));
        }

        public void Visit(ModuloNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(Expression.Modulo(left, right));
        }

        public void Visit(AddNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
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
            Nodes.Push(Expression.Equal(left, right));
        }

        public void Visit(GreaterOrEqualNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(Expression.GreaterThanOrEqual(left, right));
        }

        public void Visit(LessOrEqualNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(Expression.LessThanOrEqual(left, right));
        }

        public void Visit(GreaterNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(Expression.GreaterThan(left, right));
        }

        public void Visit(LessNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(Expression.LessThan(left, right));
        }

        public void Visit(DiffNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(Expression.NotEqual(left, right));
        }

        public void Visit(NotNode node)
        {
            Nodes.Push(Expression.Not(Nodes.Pop()));
        }

        public void Visit(LikeNode node)
        {
            //var right = Nodes.Pop();
            //var left = Nodes.Pop();
            //Operators.Like
           
            throw new NotImplementedException();
        }

        public void Visit(RLikeNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(InNode node)
        {
            
            throw new NotImplementedException();
        }

        public void Visit(FieldNode node)
        {
            /// TODO: uncomment
            var value = Nodes.Pop();
            Nodes.Push(Expression.Convert(value, node.ReturnType));

            //Nodes.Push(node.Expression);
            //throw new NotImplementedException();
            //ParameterExpression row = Expression.Parameter(typeof(IObjectResolver), "row");
            //Nodes.Push(Expression.Call(typeof(IObjectResolver).GetMethod("GetValue", new Type[1] { typeof(string) }), Expression.Constant(node.FieldName)));
        }

        public void Visit(FieldOrderedNode node)
        {
            var value = Nodes.Pop();
            Nodes.Push(value);
        }

        public void Visit(StringNode node)
        {
            Nodes.Push(Expression.Constant(node.ObjValue, node.ReturnType));
            //throw new NotImplementedException();
        }

        public void Visit(DecimalNode node)
        {
            Nodes.Push(Expression.Constant(node.ObjValue, node.ReturnType));
            //throw new NotImplementedException();
        }

        public void Visit(IntegerNode node)
        {
            Nodes.Push(Expression.Constant(node.ObjValue, node.ReturnType));
            //throw new NotImplementedException();
        }

        public void Visit(BooleanNode node)
        {
            Nodes.Push(Expression.Constant(node.ObjValue, node.ReturnType));
            //throw new NotImplementedException();
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

            ////var predicateLambda = Expression.Lambda<Func<this.inputItemType, bool>>(predicate, new ParameterExpression[] { this.inputItem });
            //Expression.Lambda(Expression.Equal(left, right), left, this.item);
            //var predicateLambda = Expression.Lambda(predicate, this.item);

            //MethodCallExpression call = Expression.Call(
            //    typeof(Queryable),
            //    "Any",
            //    new Type[] { this.itemType },
            //    input,
            //    predicateLambda);

            //Nodes.Push(Expression.Lambda(call, input));

        }

        public void Visit(AccessMethodNode node)
        {
            //Queryable.GroupBy((IQueryable<IObjectResolver>)null, x => x.GetValue("")).Select(x => x.Key)
            //Expression<Func<IGrouping<object, IObjectResolver>, object>> count = x => new { c = x.Count(), a = x.Key }
            //Expression<Func<IGrouping<object, IObjectResolver>, object>> count = x => new { c = x.Sum(, a = x.Key };

            var args = Enumerable.Range(0, node.ArgsCount).Select(x => Nodes.Pop()).Reverse().ToArray();
            var argsTypes = args.Select(x => x.Type);
            //var defaultSource = Nodes.First(x => typeof(IEnumerable<IObjectResolver>).IsAssignableFrom(x.Type));

            if (node.IsAggregateMethod)
            {
                if (this.itemType.Name == "IGrouping`2")
                {
                    if (node.Method.Name == "Count")
                    {
                        MethodCallExpression call = Expression.Call(
                            typeof(Enumerable),
                            "Count",
                            new Type[] { this.groupItemType },
                            new Expression[] { this.item });
                        Nodes.Push(call);
                    }
                    if (node.Method.Name == "Sum")
                    {
                        var selector =  Expression.Lambda(args[0], this.groupItem);
                        MethodCallExpression call = Expression.Call(
                            typeof(Enumerable),
                            "Sum",
                            new Type[] { this.groupItemType },
                            new Expression[] { this.item, selector });
                        Nodes.Push(call);
                    }
                }
                else
                {
                    if (node.Method.Name == "Count")
                    {
                        MethodCallExpression call = Expression.Call(
                        typeof(Queryable),
                        "Count",
                        new Type[] { this.itemType },
                        new Expression[] { this.input });
                        Nodes.Push(call);
                    }
                    if (node.Method.Name == "Sum")
                    {
                        var selector = Expression.Lambda(args[0], this.item);
                        MethodCallExpression call = Expression.Call(
                        typeof(Queryable),
                        "Sum",
                        new Type[] { this.itemType },
                        new Expression[] { this.input, selector });
                        Nodes.Push(call);
                    }
                }
            }
            else
            {
                var instance = node.Method.ReflectedType.GetConstructors()[0].Invoke(new object[] { });
                /// TODO: check if there can be more that one generic argument
                var method = node.Method.IsGenericMethodDefinition ?
                    node.Method.MakeGenericMethod(node.ReturnType) : node.Method;

                var parameters = method.GetParameters();
                for(int i = 0; i < parameters.Length; i++)
                {
                    var parameter = parameters[0];
                    if (parameter.ParameterType.Name == "Nullable`1")
                    {
                        if (args[i].Type.IsValueType)
                        {
                            args[i] = Expression.Convert(args[i], typeof(Nullable<>).MakeGenericType(args[i].Type));
                        }
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

        public void Visit(AccessRawIdentifierNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(IsNullNode node)
        {
            var currentValue = Nodes.Pop();
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

        public void Visit(AccessRefreshAggreationScoreNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(AccessColumnNode node)
        {
            //Expression<Func<IObjectResolver, object>> getValue = r => r.GetValue(node.Name);
            //var returnValue = Expression.Invoke(getValue, new Expression[] { this.rowExpression });
            //var castToReturnType = Expression.Convert(returnValue, node.ReturnType);
            //Nodes.Push(castToReturnType);
            
            if (itemType.Name == "IGrouping`2")
            {
                // TODO: just for testing. 
                // come with idea, how to figure out if the colum is inside aggregation function 
                // or is just column to display
                try
                {
                    var key = Expression.PropertyOrField(this.item, "Key");
                    var properyOfKey = Expression.PropertyOrField(key, node.Name);
                    Nodes.Push(properyOfKey);
                }
                catch (Exception ex)
                {
                    var groupItemProperty = Expression.PropertyOrField(this.groupItem, node.Name);
                    Nodes.Push(groupItemProperty);
                }

                return;
            }

            var property = Expression.PropertyOrField(this.item, node.Name);
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
            throw new NotImplementedException();
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

        public void StartSelect(SelectNode node)
        {
            
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

            //"item => new AnonymousType() { SelectProp = item.name, SelectProp2 = item.SelectProp2) }"
            Expression expression = Expression.Lambda(initialization, item);

            var call = Expression.Call(
                typeof(Queryable),
                "Select",
                new Type[] { this.itemType, outputItemType },
                input,
                expression);

            Nodes.Push(Expression.Lambda(call, input));

            this.itemType = outputItemType;

            //"AnonymousType input"
            this.item = Expression.Parameter(this.itemType, "inputItem");

            //"IQueryable<AnonymousType> input"
            this.input = Expression.Parameter(typeof(IQueryable<>).MakeGenericType(this.itemType), "input");
        }

        public void Visit(GroupSelectNode node)
        {
        }

        public void StartWhere(WhereNode node)
        {
            //this.itemType = expressionHelper.AnonymousTypes.Last();

            //////"AnonymousType input"
            //this.item = Expression.Parameter(this.itemType, "inputItem");

            //////"IQueryable<AnonymousType> input"
            //this.input = Expression.Parameter(typeof(IQueryable<>).MakeGenericType(this.itemType), "input");
        }

        public void Visit(WhereNode node)
        {
            var predicate =  Nodes.Pop();
            //var predicateLambda = Expression.Lambda<Func<this.inputItemType, bool>>(predicate, new ParameterExpression[] { this.inputItem });
            var predicateLambda = Expression.Lambda(predicate, this.item);

            MethodCallExpression call = Expression.Call(
                typeof(Queryable),
                "Where",
                new Type[] { this.itemType },
                input,
                predicateLambda);

            Nodes.Push(Expression.Lambda(call, input));
        }

        public void StartGroupBy(GroupByNode node)
        {
            
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
            Expression expression = Expression.Lambda(initialization, item);

            var groupByCall = Expression.Call(
                typeof(Queryable),
                "GroupBy",
                new Type[] { this.itemType, outputItemType },
                input,
                expression);

            Nodes.Push(Expression.Lambda(groupByCall, input));

            // "ItemAnonymousType"
            this.groupItemType = this.itemType;

            // "ItemAnonymousType groupItem"
            this.groupItem = Expression.Parameter(this.itemType, "groupItem");

            // "IGrouping<KeyAnonymousType, ItemAnonymousType>"
            this.itemType = typeof(IGrouping<,>).MakeGenericType(outputItemType, this.itemType);

            // "IGrouping<KeyAnonymousType, ItemAnonymousType> item"
            this.item = Expression.Parameter(this.itemType, "inputItem");

            // "IQueryable<IGrouping<KeyAnonymousType, ItemAnonymousType>> input"
            this.input = Expression.Parameter(typeof(IQueryable<>).MakeGenericType(this.itemType), "input");

            

            ///////////////////

            //ParameterExpression inputItemGroup = Expression.Parameter(typeof(IGrouping<,>).MakeGenericType(outputItemType, this.inputItemType), "inputItemGroup");
            //var selectCall = Expression.Call(
            //    typeof(Queryable),
            //    "Select",
            //    new Type[] { typeof(IGrouping<,>).MakeGenericType(outputItemType, this.inputItemType), outputItemType },
            //    groupByCall,
            //    Expression.Lambda(Expression.Property(inputItemGroup, "Key"), inputItemGroup));

            //Nodes.Push(Expression.Lambda(selectCall, input));

            ////////////////

            //var fieldsInSelect = new Expression[node.Fields.Length];
            //for (var i = 0; i < node.Fields.Length; i++)
            //    fieldsInSelect[node.Fields.Length - 1 - i] = Nodes.Pop();

            //var returnObjectInSelect = Expression.Variable(typeof(DictionaryResolver));
            //var methodAddToDictionary = typeof(Dictionary<string, object>).GetMethod("Add", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(string), typeof(object) }, null);

            //var bodyInSelect = new List<Expression>();
            //bodyInSelect.Add(Expression.Assign(returnObjectInSelect, Expression.New(typeof(DictionaryResolver))));
            //for (int i = 0; i < fieldsInSelect.Length; i++)
            //{
            //    var key = Expression.Constant(node.Fields[i].FieldName);
            //    var value = Expression.Convert(fieldsInSelect[i], typeof(object));
            //    bodyInSelect.Add(Expression.Call(returnObjectInSelect, methodAddToDictionary, key, value));
            //}
            ////return value
            //bodyInSelect.Add(returnObjectInSelect);

            //var groupBy = Expression.Block(new[] { returnObjectInSelect }, bodyInSelect);
            //var groupByLambda = Expression.Lambda<Func<IObjectResolver, IObjectResolver>>(groupBy, inputItem);

            ///////////
            ///

            //Expression<Func<GroupByNode, IQueryable<IObjectResolver>, IQueryable<IObjectResolver>>> expression = (g, s) =>
            //{
            //}
            //((IQueryable<IObjectResolver>)groupBy).GroupBy(x => new { a = x }).Select(x => x)

            //var bodyInSelect2 = new List<Expression>();
            //bodyInSelect2.Add(Expression.Assign(Expression.Property(Expression.Property(groupExpression, "Key"), "Stream"), groupExpression));
            //bodyInSelect2.Add(Expression.Property(groupExpression, "Key"));

            //var groupByCall = Expression.Call(
            //    typeof(Queryable),
            //    "GroupBy",
            //    new Type[] { typeof(IObjectResolver), typeof(IObjectResolver) },
            //    queryableSource,
            //    groupByLambda);

            //var selectLambda = Expression.Lambda<Func<IGrouping<IObjectResolver, IObjectResolver>, IObjectResolver>>(Expression.Block(bodyInSelect2), groupExpression);
            //var selectCall = Expression.Call(
            //    typeof(Queryable),
            //    "Select",
            //    new Type[] { typeof(IGrouping<IObjectResolver, IObjectResolver>), typeof(IObjectResolver) },
            //    groupByCall,
            //    selectLambda);

            //Nodes.Push(Expression.Lambda(selectCall, source));
        }

        public void Visit(HavingNode node)
        {
            var predicate = Nodes.Pop();
            //var predicateLambda = Expression.Lambda<Func<this.inputItemType, bool>>(predicate, new ParameterExpression[] { this.inputItem });
            var predicateLambda = Expression.Lambda(predicate, this.item);

            MethodCallExpression call = Expression.Call(
                typeof(Queryable),
                "Where",
                new Type[] { this.itemType },
                input,
                predicateLambda);

            Nodes.Push(Expression.Lambda(call, input));
        }        

        public void Visit(SkipNode node)
        {
            //"IQueryable<AnonymousType> input"
            this.input = Expression.Parameter(typeof(IQueryable<>).MakeGenericType(this.itemType), "input");
            

            var skipNumber = Expression.Constant((int)node.Value);

            MethodCallExpression call = Expression.Call(
                typeof(Queryable),
                "Skip",
                new Type[] { this.itemType },
                input,
                skipNumber);

            Nodes.Push(Expression.Lambda(call, input));
        }

        public void Visit(TakeNode node)
        {
            var takeNumber = Expression.Constant((int)node.Value);

            MethodCallExpression call = Expression.Call(
                typeof(Queryable),
                "Take",
                new Type[] { this.itemType },
                input,
                takeNumber);

            Nodes.Push(Expression.Lambda(call, input));

            //IQueryable<IObjectResolver> stream = Streams[_queryAlias];
            //Streams[_queryAlias] = stream.Take((int)node.Value);
        }

        public void Visit(JoinInMemoryWithSourceTableFromNode node)
        {
            throw new NotImplementedException();
        }

        
        public void Visit(SchemaFromNode node)
        {
            var rowSource = _schemaProvider.GetSchema(node.Schema).GetRowSource(node.Method, _interCommunicator, new object[0]).Rows;

            var fields = new[] {
                ("Month", typeof(string)),
                ("Name", typeof(string)),
                ("Country", typeof(string)),
                ("City", typeof(string)),
                ("Population", typeof(decimal)),
                ("Money", typeof(decimal)),
                ("Time", typeof(DateTime)),
                
                //("Time", typeof(DateTime)),
                //("Id", typeof(int)),
                //("NullableValue", typeof(int))
            };

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

            Nodes.Push(Expression.Invoke(Expression.Lambda(call)));

            this.itemType = entityType;

            //"AnonymousType input"
            this.item = Expression.Parameter(this.itemType, "inputItem");

            //"IQueryable<AnonymousType> input"
            this.input = Expression.Parameter(typeof(IQueryable<>).MakeGenericType(this.itemType), "input");

            //foreach (var row in rowSource)
            //{
            //    List<MemberBinding> bindings = new List<MemberBinding>();
            //    foreach (var field in fields)
            //    {
            //        //"SelectProp = item.name"
            //        MemberBinding assignment = Expression.Bind(entityType.GetField(field.Item1), Expression.Constant(row.GetValue(field.Item1)));
            //    }

            //    //"new AnonymousType()"
            //    var creationExpression = Expression.New(entityType.GetConstructor(Type.EmptyTypes));

            //    //"new AnonymousType() { SelectProp = item.name, SelectProp2 = item.SelectProp2) }"
            //    var initialization = Expression.MemberInit(creationExpression, bindings);

            //    //"item => new AnonymousType() { SelectProp = item.name, SelectProp2 = item.SelectProp2) }"
            //    Expression expression = Expression.Lambda(initialization);




            //Nodes.Push(Expression.Constant(rowSource.AsQueryable()));
            //Streams[node.Alias] = rowSource.AsQueryable();
            //Streams.Push(Expression.Constant(rowSource.AsQueryable()));
            //throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public void Visit(JoinFromNode node)
        {
        }

        public void Visit(ExpressionFromNode node)
        {
            //throw new NotImplementedException();
        }

        public void Visit(SchemaMethodFromNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(CreateTransformationTableNode node)
        {
            
            //throw new NotImplementedException();
        }

        public void Visit(RenameTableNode node)
        {
        }

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
            //var stream = Streams.Pop(); // Streams[_queryAlias];

            Expression select = node.Select != null ? Nodes.Pop() : null;

            Expression orderBy = node.OrderBy != null ? Nodes.Pop() : null;

            Expression take = node.Take != null ? Nodes.Pop() : null;
            Expression skip = node.Skip != null ? Nodes.Pop() : null;

            Expression having = (node.GroupBy != null && node.GroupBy.Having != null) ? Nodes.Pop() : null;

            Expression groupBy = node.GroupBy != null ? Nodes.Pop() : null;

            Expression where = node.Where != null ? Nodes.Pop() : null;

            Expression from = node.From != null ? Nodes.Pop() : null;


            Expression last = from;// Expression.Constant(stream, typeof(IQueryable<IObjectResolver>));

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
                last = Expression.Invoke(select, last);
            }

            Nodes.Push(last);
            Columns[_queryAlias] = node.Select.Fields.Select(x => x.FieldName).ToArray();
            ColumnsTypes[_queryAlias] = node.Select.Fields.Select(x => x.ReturnType).ToArray();
        }

        public void Visit(InternalQueryNode node)
        {
            //var stream = Nodes.Pop(); // Streams[_queryAlias];


            Expression skip = node.Skip != null ? Nodes.Pop() : null;
            Expression take = node.Take != null ? Nodes.Pop() : null;

            Expression select = node.Select != null ? Nodes.Pop() : null;
            Expression where = node.Where != null ? Nodes.Pop() : null;

            Expression groupBy = node.GroupBy != null ? Nodes.Pop() : null;

            Expression from = node.From != null ? Nodes.Pop() : null;


            Expression last = from;// Expression.Constant(stream, typeof(IQueryable<IObjectResolver>));

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
            //Streams.Push(last);
            //Expression<Func<IEnumerable<IObjectResolver>>> toStream = Expression.Lambda<Func<IEnumerable<IObjectResolver>>>(last);
        }

        public void Visit(RootNode node)
        {
            
            Expression last = Nodes.Pop();
            Expression<Func<IEnumerable<object>>> toStream = Expression.Lambda<Func<IEnumerable<object>>>(last);
            var compiledToStream = toStream.Compile();
            Stream = compiledToStream().AsQueryable().Select(x => new AnonymousTypeResolver(x));

            //throw new NotImplementedException();
        }

        public void Visit(SingleSetNode node)
        {
            throw new NotImplementedException();
        }

        public Expression Select(Expression input, Type outputItemType)
        {
            var inputItemType = input.Type.GetGenericArguments()[0]; //IQueryable<AnonymousType>
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

        public void Visit(UnionNode node)
        {
            var right = Nodes.Pop();
            var rightType = right.Type.GetGenericArguments()[0]; //IQueryable<AnonymousType>

            var left = Nodes.Pop();
            var leftType = left.Type.GetGenericArguments()[0]; //IQueryable<AnonymousType>

            var outputItemType = expressionHelper.CreateAnonymousTypeSameAs(leftType);
            var leftSelect = Select(left, outputItemType);
            var rightSelect = Select(right, outputItemType);

            var call = Expression.Call(
                typeof(Queryable),
                "Union",
                new Type[] { outputItemType },
                leftSelect,
                rightSelect);

            Nodes.Push(call);
        }

        public void Visit(UnionAllNode node)
        {
            var right = Nodes.Pop();
            var rightType = right.Type.GetGenericArguments()[0]; //IQueryable<AnonymousType>

            var left = Nodes.Pop();
            var leftType = left.Type.GetGenericArguments()[0]; //IQueryable<AnonymousType>

            var outputItemType = expressionHelper.CreateAnonymousTypeSameAs(leftType);
            var leftSelect = Select(left, outputItemType);
            var rightSelect = Select(right, outputItemType);

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
            var rightType = right.Type.GetGenericArguments()[0]; //IQueryable<AnonymousType>

            var left = Nodes.Pop();
            var leftType = left.Type.GetGenericArguments()[0]; //IQueryable<AnonymousType>

            var outputItemType = expressionHelper.CreateAnonymousTypeSameAs(leftType);
            var leftSelect = Select(left, outputItemType);
            var rightSelect = Select(right, outputItemType);

            var call = Expression.Call(
                typeof(Queryable),
                "Except",
                new Type[] { outputItemType },
                leftSelect,
                rightSelect);

            Nodes.Push(call);
        }

        public void Visit(RefreshNode node)
        {
            //throw new NotImplementedException();
        }

        public void Visit(IntersectNode node)
        {
            var right = Nodes.Pop();
            var rightType = right.Type.GetGenericArguments()[0]; //IQueryable<AnonymousType>

            var left = Nodes.Pop();
            var leftType = left.Type.GetGenericArguments()[0]; //IQueryable<AnonymousType>

            var outputItemType = expressionHelper.CreateAnonymousTypeSameAs(leftType);
            var leftSelect = Select(left, outputItemType);
            var rightSelect = Select(right, outputItemType);

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
            

            //throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public void Visit(CteInnerExpressionNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(JoinsNode node)
        {
            //Enumerable.Join()
            //var left = Nodes.Pop();
            //var right = Nodes.Pop();
            //var joins = node.Joins;
            //throw new NotImplementedException();
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

    }

}