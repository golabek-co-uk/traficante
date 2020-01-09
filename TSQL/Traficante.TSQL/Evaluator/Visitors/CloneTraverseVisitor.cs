using System;
using Traficante.TSQL.Parser;
using Traficante.TSQL.Parser.Nodes;

namespace Traficante.TSQL.Evaluator.Visitors
{
    public class CloneTraverseVisitor : IExpressionVisitor
    {
        protected readonly IAwareExpressionVisitor Visitor;

        protected CloneTraverseVisitor(IAwareExpressionVisitor visitor)
        {
            Visitor = visitor ?? throw new ArgumentNullException(nameof(visitor));
        }

        public virtual void Visit(SelectNode node)
        {
            Visitor.SetQueryPart(QueryPart.Select);
            foreach (var field in node.Fields)
                field.Accept(this);
            node.Accept(Visitor);
            Visitor.SetQueryPart(QueryPart.None);
        }
        public virtual void Visit(StringNode node)
        {
            node.Accept(Visitor);
        }

        public virtual void Visit(IntegerNode node)
        {
            node.Accept(Visitor);
        }

        public virtual void Visit(BooleanNode node)
        {
            node.Accept(Visitor);
        }

        public virtual void Visit(WordNode node)
        {
            node.Accept(Visitor);
        }

        public virtual void Visit(ContainsNode node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);
            node.Accept(Visitor);
        }

        public virtual void Visit(FunctionNode node)
        {
            node.Arguments.Accept(this);
            node.Accept(Visitor);
        }

        public virtual void Visit(IsNullNode node)
        {
            node.Expression.Accept(this);
            node.Accept(Visitor);
        }

        public virtual void Visit(AccessColumnNode node)
        {
            node.Accept(Visitor);
        }

        public virtual void Visit(AllColumnsNode node)
        {
            node.Accept(Visitor);
        }

        public virtual void Visit(IdentifierNode node)
        {
            node.Accept(Visitor);
        }

        public virtual void Visit(AccessObjectArrayNode node)
        {
            node.Accept(Visitor);
        }

        public virtual void Visit(AccessObjectKeyNode node)
        {
            node.Accept(Visitor);
        }

        public virtual void Visit(PropertyValueNode node)
        {
            node.Accept(Visitor);
        }

        public virtual void Visit(VariableNode node)
        {
            node.Accept(Visitor);
        }

        public virtual void Visit(DeclareNode node)
        {
            node.Accept(Visitor);
        }

        public virtual void Visit(SetNode node)
        {
            node.Accept(Visitor);
        }

        public virtual void Visit(DotNode node)
        {
            node.Root.Accept(this);
            node.Expression.Accept(this);
            node.Accept(Visitor);
        }

        public virtual void Visit(WhereNode node)
        {
            Visitor.SetQueryPart(QueryPart.Where);
            node.Expression.Accept(this);
            node.Accept(Visitor);
            Visitor.SetQueryPart(QueryPart.None);
        }

        public virtual void Visit(GroupByNode node)
        {
            Visitor.SetQueryPart(QueryPart.GroupBy);
            foreach (var field in node.Fields)
                field.Accept(this);

            node.Having?.Accept(this);
            node.Accept(Visitor);
            Visitor.SetQueryPart(QueryPart.None);
        }

        public virtual void Visit(HavingNode node)
        {
            Visitor.SetQueryPart(QueryPart.Having);
            node.Expression.Accept(this);
            node.Accept(Visitor);
            Visitor.SetQueryPart(QueryPart.None);
        }

        public virtual void Visit(SkipNode node)
        {
            node.Accept(Visitor);
        }

        public virtual void Visit(TakeNode node)
        {
            node.Accept(Visitor);
        }


        public virtual void Visit(FromFunctionNode node)
        {
            Visitor.SetQueryPart(QueryPart.From);
            node.Function.Accept(this);
            node.Accept(Visitor);
            Visitor.SetQueryPart(QueryPart.None);
        }

        public virtual void Visit(FromTableNode node)
        {
            Visitor.SetQueryPart(QueryPart.From);
            node.Accept(Visitor);
            Visitor.SetQueryPart(QueryPart.None);
        }

        public virtual void Visit(InMemoryTableFromNode node)
        {
            Visitor.SetQueryPart(QueryPart.From);
            node.Accept(Visitor);
            Visitor.SetQueryPart(QueryPart.None);
        }

        public virtual void Visit(JoinFromNode node)
        {
            Visitor.SetQueryPart(QueryPart.From);
            node.Source.Accept(this);
            node.With.Accept(this);
            node.Expression.Accept(this);
            node.Accept(Visitor);
            Visitor.SetQueryPart(QueryPart.None);
        }

        public virtual void Visit(ExpressionFromNode node)
        {
            Visitor.SetQueryPart(QueryPart.From);
            node.Expression.Accept(this);
            node.Accept(Visitor);
            Visitor.SetQueryPart(QueryPart.None);
        }

        public virtual void Visit(IntoNode node)
        {
            node.Accept(Visitor);
        }

        public virtual void Visit(QueryScope node)
        {
            node.Accept(Visitor);
        }

        public virtual void Visit(QueryNode node)
        {
            node.From.Accept(this);
            node.Where?.Accept(this);
            node.Select.Accept(this);
            node.Take?.Accept(this);
            node.Skip?.Accept(this);
            node.GroupBy?.Accept(this);
            node.OrderBy?.Accept(this);
            node.Accept(Visitor);
            Visitor.SetQueryPart(QueryPart.None);
        }

        public virtual void Visit(OrNode node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);
            node.Accept(Visitor);
        }

        public virtual void Visit(HyphenNode node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);
            node.Accept(Visitor);
        }

        public virtual void Visit(AndNode node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);
            node.Accept(Visitor);
        }

        public virtual void Visit(EqualityNode node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);
            node.Accept(Visitor);
        }

        public virtual void Visit(GreaterOrEqualNode node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);
            node.Accept(Visitor);
        }

        public virtual void Visit(LessOrEqualNode node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);
            node.Accept(Visitor);
        }

        public virtual void Visit(GreaterNode node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);
            node.Accept(Visitor);
        }

        public virtual void Visit(LessNode node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);
            node.Accept(Visitor);
        }

        public virtual void Visit(DiffNode node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);
            node.Accept(Visitor);
        }

        public virtual void Visit(NotNode node)
        {
            node.Expression.Accept(this);
            node.Accept(Visitor);
        }

        public virtual void Visit(LikeNode node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);
            node.Accept(Visitor);
        }

        public virtual void Visit(RLikeNode node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);
            node.Accept(Visitor);
        }

        public virtual void Visit(InNode node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);
            node.Accept(Visitor);
        }

        public virtual void Visit(FieldNode node)
        {
            node.Expression.Accept(this);
            node.Accept(Visitor);
        }

        public virtual void Visit(FieldOrderedNode node)
        {
            node.Expression.Accept(this);
            node.Accept(Visitor);
        }

        public virtual void Visit(ArgsListNode node)
        {
            foreach (var item in node.Args)
                item.Accept(this);
            node.Accept(Visitor);
        }

        public virtual void Visit(DecimalNode node)
        {
            node.Accept(Visitor);
        }

        public virtual void Visit(Node node)
        {
            throw new NotSupportedException();
        }

        public virtual void Visit(DescNode node)
        {
            node.From.Accept(this);
            node.Accept(Visitor);
        }

        public virtual void Visit(StarNode node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);
            node.Accept(Visitor);
        }

        public virtual void Visit(FSlashNode node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);
            node.Accept(Visitor);
        }

        public virtual void Visit(ModuloNode node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);
            node.Accept(Visitor);
        }

        public virtual void Visit(AddNode node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);
            node.Accept(Visitor);
        }

        public virtual void Visit(RootNode node)
        {
            node.Expression.Accept(this);
            node.Accept(Visitor);
        }

        public virtual void Visit(SingleSetNode node)
        {
            node.Query.Accept(this);
            node.Accept(Visitor);
        }

        public virtual void Visit(UnionNode node)
        {
            TraverseSetOperator(node);
        }

        public virtual void Visit(UnionAllNode node)
        {
            TraverseSetOperator(node);
        }

        public virtual void Visit(ExceptNode node)
        {
            TraverseSetOperator(node);
        }

        public virtual void Visit(IntersectNode node)
        {
            TraverseSetOperator(node);
        }

        public virtual void Visit(PutTrueNode node)
        {
            node.Accept(Visitor);
        }

        public virtual void Visit(MultiStatementNode node)
        {
            foreach (var cNode in node.Nodes)
                cNode.Accept(this);
            node.Accept(Visitor);
        }

        public virtual void Visit(CteExpressionNode node)
        {
            foreach (var exp in node.InnerExpression) exp.Accept(this);
            node.OuterExpression.Accept(this);
            node.Accept(Visitor);
        }

        public virtual void Visit(CteInnerExpressionNode node)
        {
            node.Value.Accept(this);
            node.Accept(Visitor);
        }

        public virtual void Visit(JoinsNode node)
        {
            node.Joins.Accept(this);
            node.Accept(Visitor);
        }

        public virtual void Visit(JoinNode node)
        {
            node.From.Accept(this);
            node.Expression.Accept(this);
            node.Accept(Visitor);
        }

        public virtual void Visit(FromNode node)
        {
            Visitor.SetQueryPart(QueryPart.From);
            node.Accept(Visitor);
            Visitor.SetQueryPart(QueryPart.None);
        }

        public virtual void Visit(OrderByNode node)
        {
            foreach (var field in node.Fields)
                field.Accept(this);

            node.Accept(Visitor);
        }

        public void Visit(CreateTableNode node)
        {
            node.Accept(Visitor);
        }

        public void Visit(StatementsArrayNode node)
        {
            foreach (var statement in node.Statements)
                statement.Accept(this);

            node.Accept(Visitor);
        }

        public void Visit(StatementNode node)
        {
            node.Node.Accept(this);
            node.Accept(Visitor);
        }

        public void Visit(CaseNode node)
        {
            node.Else.Accept(this);

            for (int i = node.WhenThenPairs.Length - 1; i >= 0; --i)
            {
                node.WhenThenPairs[i].When.Accept(this);
                node.WhenThenPairs[i].Then.Accept(this);
            }

            node.Accept(Visitor);
        }

        public void Visit(TypeNode node)
        {
            node.Accept(Visitor);
        }

        public void Visit(ExecuteNode node)
        {
            node.VariableToSet?.Accept(Visitor);
            node.FunctionToRun?.Accept(Visitor);
            node.Accept(Visitor);
        }

        private void TraverseSetOperator(SetOperatorNode node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);
            node.Accept(Visitor);
        }
    }
}