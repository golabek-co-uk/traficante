using Traficante.TSQL.Evaluator.Visitors;

namespace Traficante.TSQL.Parser.Nodes
{
    public class FromSubQueryNode : FromNode
    {
        public FromSubQueryNode(StatementNode subQuery, string alias)
            : base(alias)
        {
            SubQuery = subQuery;
            Id = $"{nameof(FromSubQueryNode)}{subQuery.Id}{Alias}";
        }

        public StatementNode SubQuery { get; set; }

        public override string Id { get; }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {

            return $"from {SubQuery} {Alias}";
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is FromSubQueryNode node)
                return node.Id == Id;

            return base.Equals(obj);
        }
    }
}