using Traficante.TSQL.Evaluator.Visitors;

namespace Traficante.TSQL.Parser.Nodes
{
    public class JoinNode : FromNode
    {
        public JoinNode(FromNode source, FromNode with, Node expression, JoinType joinType)
            : base($"{source.Alias}{with.Alias}")
        {
            Source = source;
            With = with;
            Expression = expression;
            JoinType = joinType;
        }

        public FromNode Source { get; }
        public FromNode With { get; }
        public Node Expression { get; }
        public JoinType JoinType { get; }
        public JoinOperator? JoinOperator { get; private set; }
        public override string Id => $"{typeof(JoinNode)}{Source.Id}{With.Id}{Expression.Id}";

        public void ChangeJoinOperator(JoinOperator joinOperator)
        {
            JoinOperator = joinOperator;
        }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"({Source.ToString()}, {With.ToString()}, {Expression.ToString()})";
        }
    }
}