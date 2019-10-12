namespace Traficante.TSQL.Parser.Nodes
{
    public class FromTableNode : FromNode
    {
        public FromTableNode(TableNode table, string alias)
            : base(alias)
        {
            Table = table;
            Id = $"{nameof(FromTableNode)}{table.Id}{Alias}";
        }

        public TableNode Table { get; set; }

        public override string Id { get; }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {

            return $"from {Table} {Alias}";
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is FromTableNode node)
                return node.Id == Id;

            return base.Equals(obj);
        }
    }

}