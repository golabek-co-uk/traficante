using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Traficante.TSQL.Evaluator.Exceptions;
using Traficante.TSQL.Evaluator.Helpers;
using Traficante.TSQL.Evaluator.Tables;
using Traficante.TSQL.Evaluator.Utils;
using Traficante.TSQL.Evaluator.Utils.Symbols;
using Traficante.TSQL.Parser;
using Traficante.TSQL.Parser.Nodes;
using Traficante.TSQL.Parser.Tokens;
using Traficante.TSQL.Lib.Attributes;
using Traficante.TSQL.Schema;
using Traficante.TSQL.Schema.DataSources;
using Traficante.TSQL.Schema.Helpers;

namespace Traficante.TSQL.Evaluator.Visitors
{
    public class BuildMetadataAndInferTypeVisitor : CloneQueryVisitor
    {
        private readonly Engine _engine;

        private readonly List<string> _generatedAliases = new List<string>();
        private FieldNode[] _generatedColumns = new FieldNode[0];
        private string _identifier;
        private string _queryAlias;


        private Stack<string> Methods { get; } = new Stack<string>();

        public BuildMetadataAndInferTypeVisitor(Engine engine)
        {
            _engine = engine;
        }

        public override void Visit(SelectNode node)
        {
            var fields = CreateFields(node.Fields);

            Nodes.Push(new SelectNode(fields.ToArray(), node.ReturnsSingleRow));
        }

        public override void Visit(FunctionNode node)
        {
            var args = Nodes.Pop() as ArgsListNode;

            var methodInfo = this._engine.ResolveMethod(node.Path, node.Name, args.Args.Select(f => f.ReturnType).ToArray());
            FunctionNode functionMethod = new FunctionNode(node.Name, args, node.Path, methodInfo);
            Nodes.Push(functionMethod);
        }

        public override void Visit(AccessColumnNode node)
        {
            var identifier = CurrentScope.ContainsAttribute("ProcessedQueryId")
                ? CurrentScope["ProcessedQueryId"]
                : _identifier;

            var tableSymbol = CurrentScope.ScopeSymbolTable.GetSymbol<TableSymbol>(identifier);

            (Table Table, string TableName) tuple;
            if (!string.IsNullOrEmpty(node.Alias))
                tuple = tableSymbol.GetTableByAlias(node.Alias);
            else
                tuple = tableSymbol.GetTableByColumnName(node.Name);

            var column = tuple.Table.Columns.SingleOrDefault(f => f.ColumnName == node.Name);

            node.ChangeReturnType(column.ColumnType);

            var accessColumn = new AccessColumnNode(column.ColumnName, tuple.TableName, column.ColumnType, node.Span);
            Nodes.Push(accessColumn);
        }

        public override void Visit(AllColumnsNode node)
        {
            var tableSymbol = CurrentScope.ScopeSymbolTable.GetSymbol<TableSymbol>(_identifier);
            var tuple = tableSymbol.GetTableByAlias(_identifier);
            var table = tuple.Table;
            _generatedColumns = new FieldNode[table.Columns.Length];

            for (var i = 0; i < table.Columns.Length; i++)
            {
                var column = table.Columns[i];

                _generatedColumns[i] =
                    new FieldNode(
                        new AccessColumnNode(column.ColumnName, _identifier, column.ColumnType, TextSpan.Empty), i,
                        tableSymbol.HasAlias ? _identifier : column.ColumnName);
            }

            Nodes.Push(node);
        }

        public override void Visit(IdentifierNode node)
        {
            if (node.Name != _identifier)
            {
                var tableSymbol = CurrentScope.ScopeSymbolTable.GetSymbol<TableSymbol>(_identifier);
                var column = tableSymbol.GetColumnByAliasAndName(_identifier, node.Name);
                Visit(new AccessColumnNode(node.Name, string.Empty, column.ColumnType, TextSpan.Empty));
                return;
            }

            Nodes.Push(new IdentifierNode(node.Name));
        }

        public override void Visit(VariableNode node)
        {
            var variable = _engine.GetVariable(node.Name);
            if (variable == null)
                throw new Exception("Variable is not declare: " + node.Name);
            Nodes.Push(new VariableNode(node.Name, variable.Type));
        }

        public override void Visit(DeclareNode node)
        {
            _engine.SetVariable(node.Variable.Name, node.Type.ReturnType, null);
            Nodes.Push(new DeclareNode(node.Variable, node.Type));
        }

        public override void Visit(FromFunctionNode node)
        {
            var functionNode = node.Function;
            var method = _engine.ResolveMethod(functionNode.Path, functionNode.Name, functionNode.ArgumentsTypes);
            var returnType = method.FunctionMethod.ReturnType;
            functionNode = new FunctionNode(functionNode.Name, functionNode.Arguments, functionNode.Path, method);
            var columns = TypeHelper.GetColumns(returnType);
            Table table = new Table(functionNode.Name, functionNode.Path, columns);

            _queryAlias = StringHelpers.CreateAliasIfEmpty(node.Alias, _generatedAliases);
            _generatedAliases.Add(_queryAlias);

            var tableSymbol = new TableSymbol(functionNode.Path, _queryAlias, table, !string.IsNullOrEmpty(node.Alias));
            CurrentScope.ScopeSymbolTable.AddSymbol(_queryAlias, tableSymbol);
            CurrentScope[node.Id] = _queryAlias;

            var aliasedSchemaFromNode = new FromFunctionNode(functionNode, _queryAlias);
            Nodes.Push(aliasedSchemaFromNode);
        }

        public override void Visit(InMemoryTableFromNode node)
        {
            _queryAlias = string.IsNullOrEmpty(node.Alias) ? node.VariableName : node.Alias;
            _generatedAliases.Add(_queryAlias);

            TableSymbol tableSymbol;

            if (CurrentScope.Parent.ScopeSymbolTable.SymbolIsOfType<TableSymbol>(node.VariableName))
            {
                tableSymbol = CurrentScope.Parent.ScopeSymbolTable.GetSymbol<TableSymbol>(node.VariableName);
            }
            else
            {
                var scope = CurrentScope;
                while (scope != null && scope.Name != "CTE") scope = scope.Parent;

                tableSymbol = scope.ScopeSymbolTable.GetSymbol<TableSymbol>(node.VariableName);
            }

            var tableSchemaPair = tableSymbol.GetTableByAlias(node.VariableName);
            CurrentScope.ScopeSymbolTable.AddSymbol(_queryAlias,
                new TableSymbol(null, _queryAlias, tableSchemaPair.Table, node.Alias == _queryAlias));
            CurrentScope[node.Id] = _queryAlias;

            Nodes.Push(new InMemoryTableFromNode(node.VariableName, _queryAlias));
        }

        public override void Visit(FromTableNode node)
        {
            TableSymbol tableSymbol = null;

            Table table = null;
            var dbTable = _engine.ResolveTable(node.Table.TableOrView, node.Table.Path);
            if (dbTable != null)
            {
                table = new Table(dbTable.Name, node.Table.Path, TypeHelper.GetColumns(dbTable.ItemsType));
            }
            else
            {
                _queryAlias = string.IsNullOrEmpty(node.Alias) ? node.Table.TableOrView : node.Alias;
                _generatedAliases.Add(_queryAlias);

                if (CurrentScope.Parent.ScopeSymbolTable.SymbolIsOfType<TableSymbol>(node.Table.TableOrView))
                {
                    tableSymbol = CurrentScope.Parent.ScopeSymbolTable.GetSymbol<TableSymbol>(node.Table.TableOrView);
                }
                else
                {
                    var scope = CurrentScope;
                    while (scope != null && scope.Name != "CTE") scope = scope.Parent;

                    tableSymbol = scope.ScopeSymbolTable.GetSymbol<TableSymbol>(node.Table.TableOrView);
                }

                var tableSchemaPair = tableSymbol.GetTableByAlias(node.Table.TableOrView);
                CurrentScope.ScopeSymbolTable.AddSymbol(_queryAlias,
                    new TableSymbol(null, _queryAlias, tableSchemaPair.Table, node.Alias == _queryAlias));
                CurrentScope[node.Id] = _queryAlias;

                Nodes.Push(new InMemoryTableFromNode(node.Table.TableOrView, _queryAlias));
                return;
            }

            _queryAlias = StringHelpers.CreateAliasIfEmpty(node.Alias, _generatedAliases);
            _generatedAliases.Add(_queryAlias);

            tableSymbol = new TableSymbol(node.Table.Path, _queryAlias, table, !string.IsNullOrEmpty(node.Alias));
            CurrentScope.ScopeSymbolTable.AddSymbol(_queryAlias, tableSymbol);
            CurrentScope[node.Id] = _queryAlias;

            var aliasedSchemaFromNode = new FromTableNode(node.Table, _queryAlias);


            Nodes.Push(aliasedSchemaFromNode);
        }

        public override void Visit(JoinFromNode node)
        {
            var expression = Nodes.Pop();
            var joinedTable = (FromNode)Nodes.Pop();
            var source = (FromNode)Nodes.Pop();
            var joinedFrom = new JoinFromNode(source, joinedTable, expression, node.JoinType);
            _identifier = joinedFrom.Alias;
            Nodes.Push(joinedFrom);
        }

        public override void Visit(ExpressionFromNode node)
        {
            var from = (FromNode)Nodes.Pop();
            _identifier = from.Alias;
            Nodes.Push(new ExpressionFromNode(from));
        }

        public override void Visit(QueryNode node)
        {
            var orderBy = node.OrderBy != null ? Nodes.Pop() as OrderByNode : null;
            var groupBy = node.GroupBy != null ? Nodes.Pop() as GroupByNode : null;

            var skip = node.Skip != null ? Nodes.Pop() as SkipNode : null;
            var take = node.Take != null ? Nodes.Pop() as TakeNode : null;

            var select = Nodes.Pop() as SelectNode;
            var where = node.Where != null ? Nodes.Pop() as WhereNode : null;
            var from = node.From != null ? Nodes.Pop() as FromNode : null;

            if (from != null)
                Methods.Push(from.Alias);

            if (from == null)
                select.ReturnsSingleRow = true;

            if (groupBy == null)
            {
                var split = SplitBetweenAggreateAndNonAggreagate(select.Fields);
                if (split.NotAggregateFields.Length == 0)
                {
                    select.ReturnsSingleRow = true;
                }
            }

            Nodes.Push(new QueryNode(select, from, where, groupBy, orderBy, skip, take));
        }

        private (FieldNode[] AggregateFields, FieldNode[] NotAggregateFields) SplitBetweenAggreateAndNonAggreagate(FieldNode[] fieldsToSplit)
        {
            var aggregateFields = new List<FieldNode>();
            var notAggregateFields = new List<FieldNode>();

            foreach (var root in fieldsToSplit)
            {
                var subNodes = new Stack<Node>();

                subNodes.Push(root.Expression);
                bool hasAggregateMethod = false;
                while (subNodes.Count > 0)
                {
                    var subNode = subNodes.Pop();

                    if (subNode is FunctionNode aggregateMethod && aggregateMethod.IsAggregateMethod)
                    {
                        hasAggregateMethod = true;
                        break;
                    }
                    else
                    if (subNode is FunctionNode method)
                    {
                        foreach (var arg in method.Arguments.Args)
                            subNodes.Push(arg);
                    }
                    else if (subNode is BinaryNode binary)
                    {
                        subNodes.Push(binary.Left);
                        subNodes.Push(binary.Right);
                    }
                }
                if (hasAggregateMethod)
                    aggregateFields.Add(root);
                else
                    notAggregateFields.Add(root);
            }

            return (aggregateFields.ToArray(), notAggregateFields.ToArray());
        }


        public override void Visit(UnionNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();

            var rightMethodName = Methods.Pop();
            var leftMethodName = Methods.Pop();

            var methodName = $"{leftMethodName}_Union_{rightMethodName}";
            Methods.Push(methodName);
            CurrentScope.ScopeSymbolTable.AddSymbol(methodName,
                CurrentScope.Child[0].ScopeSymbolTable.GetSymbol(((QueryNode)left).From.Alias));

            Nodes.Push(new UnionNode(node.ResultTableName, node.Keys, left, right, node.IsNested, node.IsTheLastOne));
        }

        public override void Visit(UnionAllNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();

            var rightMethodName = Methods.Pop();
            var leftMethodName = Methods.Pop();

            var methodName = $"{leftMethodName}_UnionAll_{rightMethodName}";
            Methods.Push(methodName);
            CurrentScope.ScopeSymbolTable.AddSymbol(methodName,
                CurrentScope.Child[0].ScopeSymbolTable.GetSymbol(((QueryNode)left).From.Alias));

            Nodes.Push(new UnionAllNode(node.ResultTableName, node.Keys, left, right, node.IsNested,
                node.IsTheLastOne));
        }

        public override void Visit(ExceptNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();

            var rightMethodName = Methods.Pop();
            var leftMethodName = Methods.Pop();

            var methodName = $"{leftMethodName}_Except_{rightMethodName}";
            Methods.Push(methodName);
            CurrentScope.ScopeSymbolTable.AddSymbol(methodName,
                CurrentScope.Child[0].ScopeSymbolTable.GetSymbol(((QueryNode)left).From.Alias));

            Nodes.Push(new ExceptNode(node.ResultTableName, node.Keys, left, right, node.IsNested, node.IsTheLastOne));
        }

        public override void Visit(IntersectNode node)
        {

            var right = Nodes.Pop();
            var left = Nodes.Pop();

            var rightMethodName = Methods.Pop();
            var leftMethodName = Methods.Pop();

            var methodName = $"{leftMethodName}_Intersect_{rightMethodName}";
            Methods.Push(methodName);
            CurrentScope.ScopeSymbolTable.AddSymbol(methodName,
                CurrentScope.Child[0].ScopeSymbolTable.GetSymbol(((QueryNode)left).From.Alias));

            Nodes.Push(
                new IntersectNode(node.ResultTableName, node.Keys, left, right, node.IsNested, node.IsTheLastOne));
        }

        public override void Visit(CteExpressionNode node)
        {
            var sets = new CteInnerExpressionNode[node.InnerExpression.Length];

            var set = Nodes.Pop();

            for (var i = node.InnerExpression.Length - 1; i >= 0; --i)
                sets[i] = (CteInnerExpressionNode)Nodes.Pop();

            Nodes.Push(new CteExpressionNode(sets, set));
        }

        public override void Visit(CteInnerExpressionNode node)
        {
            var set = Nodes.Pop();

            var collector = new GetSelectFieldsVisitor();
            var traverser = new GetSelectFieldsTraverseVisitor(collector);

            set.Accept(traverser);

            var table = new Table(node.Name, new string[0], collector.CollectedFieldNames);
            CurrentScope.Parent.ScopeSymbolTable.AddSymbol(node.Name,
                new TableSymbol(new string[0], node.Name, table, false));

            Nodes.Push(new CteInnerExpressionNode(set, node.Name));
        }

        public override void Visit(JoinsNode node)
        {
            _identifier = node.Alias;
            Nodes.Push(new JoinsNode((JoinFromNode)Nodes.Pop()));
        }

        public override void Visit(JoinNode node)
        {
            var expression = Nodes.Pop();
            var fromNode = (FromNode)Nodes.Pop();

            if (node is OuterJoinNode outerJoin)
                Nodes.Push(new OuterJoinNode(outerJoin.Type, fromNode, expression));
            else
                Nodes.Push(new InnerJoinNode(fromNode, expression));
        }

        private FieldNode[] CreateFields(FieldNode[] oldFields)
        {
            var reorderedList = new FieldNode[oldFields.Length];
            var fields = new List<FieldNode>(reorderedList.Length);

            for (var i = reorderedList.Length - 1; i >= 0; i--) reorderedList[i] = Nodes.Pop() as FieldNode;


            for (int i = 0, j = reorderedList.Length, p = 0; i < j; ++i)
            {
                var field = reorderedList[i];

                if (field.Expression is AllColumnsNode)
                {
                    fields.AddRange(_generatedColumns.Select(column =>
                        new FieldNode(column.Expression, p++, column.FieldName)));
                    continue;
                }

                fields.Add(new FieldNode(field.Expression, p++, field.FieldName));
            }

            return fields.ToArray();
        }

        public override void Visit(OrderByNode node)
        {
            var fields = new FieldOrderedNode[node.Fields.Length];

            for (var i = node.Fields.Length - 1; i >= 0; --i)
                fields[i] = (FieldOrderedNode)Nodes.Pop();

            Nodes.Push(new OrderByNode(fields));
        }

        public override void Visit(CreateTableNode node)
        {
            var columns = new List<Column>();

            for (int i = 0; i < node.TableTypePairs.Length; i++)
            {
                (string ColumnName, string TypeName) typePair = node.TableTypePairs[i];

                var remappedType = EvaluationHelper.RemapPrimitiveTypes(typePair.TypeName);

                var type = EvaluationHelper.GetType(remappedType);

                if (type == null)
                    throw new TypeNotFoundException($"Type '{remappedType}' could not be found.");

                columns.Add(new Column(typePair.ColumnName, i, type));
            }

            Nodes.Push(new CreateTableNode(node.Name, node.TableTypePairs));
        }

    }
}