using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Traficante.TSQL.Evaluator.Utils;
using Traficante.TSQL.Parser;
using Traficante.TSQL.Parser.Nodes;

namespace Traficante.TSQL.Evaluator.Visitors
{
    public class CloneQueryVisitor : IAwareExpressionVisitor
    {
        protected Scope CurrentScope { get; set; }
        protected QueryPart QueryPart { get; set; }

        protected Stack<Node> Nodes { get; } = new Stack<Node>();

        public RootNode Root => (RootNode) Nodes.Peek();

        public virtual void Visit(Node node)
        {
        }

        public virtual void Visit(DescNode node)
        {
            Nodes.Push(new DescNode((FromNode) Nodes.Pop(), node.Type));
        }

        public virtual void Visit(StarNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(new StarNode(left, right));
        }

        public virtual void Visit(FSlashNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(new FSlashNode(left, right));
        }

        public virtual void Visit(ModuloNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(new ModuloNode(left, right));
        }

        public virtual void Visit(AddNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(new AddNode(left, right));
        }

        public virtual void Visit(HyphenNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(new HyphenNode(left, right));
        }

        public virtual void Visit(AndNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(new AndNode(left, right));
        }

        public virtual void Visit(OrNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(new OrNode(left, right));
        }

        public virtual void Visit(EqualityNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(new EqualityNode(left, right));
        }

        public virtual void Visit(GreaterOrEqualNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(new GreaterOrEqualNode(left, right));
        }

        public virtual void Visit(LessOrEqualNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(new LessOrEqualNode(left, right));
        }

        public virtual void Visit(GreaterNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(new GreaterNode(left, right));
        }

        public virtual void Visit(LessNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(new LessNode(left, right));
        }

        public virtual void Visit(DiffNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(new DiffNode(left, right));
        }

        public virtual void Visit(NotNode node)
        {
            Nodes.Push(new NotNode(Nodes.Pop()));
        }

        public virtual void Visit(LikeNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(new LikeNode(left, right));
        }

        public virtual void Visit(RLikeNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(new RLikeNode(left, right));
        }

        public virtual void Visit(InNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(new InNode(left, (ArgsListNode)right));
        }

        public virtual void Visit(FieldNode node)
        {
            Nodes.Push(new FieldNode(Nodes.Pop(), node.FieldOrder, node.FieldName));
        }

        public virtual void Visit(FieldOrderedNode node)
        {
            Nodes.Push(new FieldOrderedNode(Nodes.Pop(), node.FieldOrder, node.FieldName, node.Order));
        }

        public virtual void Visit(SelectNode node)
        {
            var fields = new FieldNode[node.Fields.Length];

            for (var i = node.Fields.Length - 1; i >= 0; --i)
                fields[i] = (FieldNode) Nodes.Pop();

            Nodes.Push(new SelectNode(fields.ToArray()));
        }

        public virtual void Visit(StringNode node)
        {
            Nodes.Push(new StringNode(node.Value));
        }

        public virtual void Visit(DecimalNode node)
        {
            Nodes.Push(new DecimalNode(node.Value));
        }

        public virtual void Visit(IntegerNode node)
        {
            Nodes.Push(new IntegerNode(node.ObjValue.ToString()));
        }

        public virtual void Visit(BooleanNode node)
        {
            Nodes.Push(new BooleanNode(node.Value));
        }

        public virtual void Visit(WordNode node)
        {
            Nodes.Push(new WordNode(node.Value));
        }

        public virtual void Visit(ContainsNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(new ContainsNode(left, right as ArgsListNode));
        }

        public virtual void Visit(FunctionNode node)
        {
            Nodes.Push(new FunctionNode(node.Name, (ArgsListNode) Nodes.Pop(), node.Path, node.Method));
        }

        public virtual void Visit(IsNullNode node)
        {
            Nodes.Push(new IsNullNode(Nodes.Pop(), node.IsNegated));
        }

        public virtual void Visit(AccessColumnNode node)
        {
            Nodes.Push(new AccessColumnNode(node.Name, node.Alias, node.ReturnType, node.Span));
        }

        public virtual void Visit(AllColumnsNode node)
        {
            Nodes.Push(new AllColumnsNode());
        }

        public virtual void Visit(IdentifierNode node)
        {
            Nodes.Push(new IdentifierNode(node.Name));
        }

        public virtual void Visit(AccessObjectArrayNode node)
        {
            var parentNodeType = Nodes.Peek().ReturnType;
            Nodes.Push(new AccessObjectArrayNode(node.Token, parentNodeType.GetProperty(node.Name)));
        }

        public virtual void Visit(AccessObjectKeyNode node)
        {
            var parentNodeType = Nodes.Peek().ReturnType;
            Nodes.Push(new AccessObjectKeyNode(node.Token, parentNodeType.GetProperty(node.ObjectName)));
        }

        public virtual void Visit(PropertyValueNode node)
        {
            Nodes.Push(new PropertyValueNode(node.Name));
            //var parentNodeType = Nodes.Peek().ReturnType;
            //Nodes.Push(new PropertyValueNode(node.Name, parentNodeType.GetProperty(node.Name)));
        }

        public virtual void Visit(VariableNode node)
        {
            Nodes.Push(new VariableNode(node.Name, node.ReturnType));
        }

        public virtual void Visit(DeclareNode node)
        {
            Nodes.Push(new DeclareNode(node.Variable, node.Type));
        }

        public virtual void Visit(SetNode node)
        {
            var value = Nodes.Pop();
            var variable = (VariableNode)Nodes.Pop();
            Nodes.Push(new SetNode(variable, value));
        }

        public virtual void Visit(DotNode node)
        {
            var exp = Nodes.Pop();
            var root = Nodes.Pop();

            Nodes.Push(new DotNode(root, exp, node.IsOuter, string.Empty));
        }

        public virtual void Visit(ArgsListNode node)
        {
            var args = new Node[node.Args.Length];

            for (var i = node.Args.Length - 1; i >= 0; --i)
                args[i] = Nodes.Pop();

            Nodes.Push(new ArgsListNode(args));
        }

        public virtual void Visit(WhereNode node)
        {
            Nodes.Push(new WhereNode(Nodes.Pop()));
        }

        public virtual void Visit(GroupByNode node)
        {
            var having = Nodes.Peek() as HavingNode;

            if (having != null)
                Nodes.Pop();

            var fields = new FieldNode[node.Fields.Length];

            for (var i = node.Fields.Length - 1; i >= 0; --i) fields[i] = Nodes.Pop() as FieldNode;

            Nodes.Push(new GroupByNode(fields, having));
        }

        public virtual void Visit(HavingNode node)
        {
            Nodes.Push(new HavingNode(Nodes.Pop()));
        }

        public virtual void Visit(SkipNode node)
        {
            Nodes.Push(new SkipNode((IntegerNode) node.Expression));
        }

        public virtual void Visit(TakeNode node)
        {
            Nodes.Push(new TakeNode((IntegerNode) node.Expression));
        }

        public virtual void Visit(FromFunctionNode node)
        {
            Nodes.Push(new FromFunctionNode((FunctionNode)Nodes.Pop(), node.Alias));
        }

        public virtual void Visit(FromTableNode node)
        {
            Nodes.Push(new FromTableNode(node.Table, node.Alias));
        }

        public virtual void Visit(InMemoryTableFromNode node)
        {
            Nodes.Push(new InMemoryTableFromNode(node.VariableName, node.Alias));
        }

        public virtual void Visit(JoinFromNode node)
        {
            var expression = Nodes.Pop();
            var joinedTable = (FromNode) Nodes.Pop();
            var source = (FromNode) Nodes.Pop();
            var joinedFrom = new JoinFromNode(source, joinedTable, expression, node.JoinType);
            Nodes.Push(joinedFrom);
        }

        public virtual void Visit(ExpressionFromNode node)
        {
            var from = (FromNode) Nodes.Pop();
            Nodes.Push(new ExpressionFromNode(from));
        }

        public virtual void Visit(IntoNode node)
        {
            Nodes.Push(new IntoNode(node.Name));
        }

        public virtual void Visit(QueryScope node)
        {
        }

        public virtual void Visit(QueryNode node)
        {
            var orderBy = node.OrderBy != null ? Nodes.Pop() as OrderByNode : null;
            var groupBy = node.GroupBy != null ? Nodes.Pop() as GroupByNode : null;

            var skip = node.Skip != null ? Nodes.Pop() as SkipNode : null;
            var take = node.Take != null ? Nodes.Pop() as TakeNode : null;

            var select = Nodes.Pop() as SelectNode;
            var where = node.Where != null ? Nodes.Pop() as WhereNode : null;
            var from = Nodes.Pop() as FromNode;

            Nodes.Push(new QueryNode(select, from, where, groupBy, orderBy, skip, take));
        }

        public virtual void Visit(RootNode node)
        {
            Nodes.Push(new RootNode(Nodes.Pop()));
        }

        public virtual void Visit(SingleSetNode node)
        {
        }

        public virtual void Visit(UnionNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();

            Nodes.Push(new UnionNode(node.ResultTableName, node.Keys, left, right, node.IsNested, node.IsTheLastOne));
        }

        public virtual void Visit(UnionAllNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();

            Nodes.Push(new UnionAllNode(node.ResultTableName, node.Keys, left, right, node.IsNested,
                node.IsTheLastOne));
        }

        public virtual void Visit(ExceptNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(new ExceptNode(node.ResultTableName, node.Keys, left, right, node.IsNested, node.IsTheLastOne));
        }

        public virtual void Visit(IntersectNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(
                new IntersectNode(node.ResultTableName, node.Keys, left, right, node.IsNested, node.IsTheLastOne));
        }

        public virtual void Visit(PutTrueNode node)
        {
            Nodes.Push(new PutTrueNode());
        }

        public virtual void Visit(MultiStatementNode node)
        {
            var items = new Node[node.Nodes.Length];

            for (var i = node.Nodes.Length - 1; i >= 0; --i)
                items[i] = Nodes.Pop();

            Nodes.Push(new MultiStatementNode(items, node.ReturnType));
        }

        public virtual void Visit(CteExpressionNode node)
        {
            var sets = new CteInnerExpressionNode[node.InnerExpression.Length];

            for (var i = node.InnerExpression.Length - 1; i >= 0; --i)
                sets[i] = (CteInnerExpressionNode) Nodes.Pop();

            Nodes.Push(new CteExpressionNode(sets, Nodes.Pop()));
        }

        public virtual void Visit(CteInnerExpressionNode node)
        {
            Nodes.Push(new CteInnerExpressionNode(Nodes.Pop(), node.Name));
        }

        public virtual void Visit(JoinsNode node)
        {
            Nodes.Push(new JoinsNode((JoinFromNode) Nodes.Pop()));
        }

        public virtual void Visit(JoinNode node)
        {
            var expression = Nodes.Pop();
            var fromNode = (FromNode) Nodes.Pop();

            if (node is OuterJoinNode outerJoin)
                Nodes.Push(new OuterJoinNode(outerJoin.Type, fromNode, expression));
            else
                Nodes.Push(new InnerJoinNode(fromNode, expression));
        }

        public virtual void Visit(OrderByNode node)
        {
            var fields = new FieldOrderedNode[node.Fields.Length];

            for (var i = node.Fields.Length - 1; i >= 0; --i)
                fields[i] = (FieldOrderedNode)Nodes.Pop();

            Nodes.Push(new OrderByNode(fields));
        }

        public virtual void Visit(CreateTableNode node)
        {
            Nodes.Push(new CreateTableNode(node.Name, node.TableTypePairs));
        }

        public virtual void Visit(StatementsArrayNode node)
        {
            var statements = new StatementNode[node.Statements.Length];
            for(int i = 0; i < node.Statements.Length; ++i)
            {
                statements[node.Statements.Length - 1 - i] = (StatementNode)Nodes.Pop();
            }

            Nodes.Push(new StatementsArrayNode(statements));
        }

        public virtual void Visit(StatementNode node)
        {
            Nodes.Push(new StatementNode(Nodes.Pop()));
        }

        public virtual void Visit(CaseNode node)
        {
            var whenThenPairs = new List<(Node When, Node Then)>();

            for (int i = 0; i < node.WhenThenPairs.Length; ++i)
            {
                var then = Nodes.Pop();
                var when = Nodes.Pop();
                whenThenPairs.Add((when, then));
            }

            var elseNode = Nodes.Pop();

            Nodes.Push(new CaseNode(whenThenPairs.ToArray(), elseNode, elseNode.ReturnType));
        }

        public virtual void Visit(TypeNode node)
        {
            Nodes.Push(new TypeNode(node.Name, node.Size));
        }

        public virtual void Visit(ExecuteNode node)
        {
            FunctionNode functionToRun = node.FunctionToRun != null ? (FunctionNode)Nodes.Pop() : null;
            VariableNode variableToSet = node.VariableToSet != null ? (VariableNode)Nodes.Pop() : null;
            Nodes.Push(new ExecuteNode(variableToSet, functionToRun));
        }

        public void SetScope(Scope scope)
        {
            CurrentScope = scope;
        }

        public void SetQueryPart(QueryPart part)
        {
            QueryPart = part;
        }
    }
}