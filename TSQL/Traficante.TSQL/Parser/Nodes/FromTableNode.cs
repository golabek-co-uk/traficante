namespace Traficante.TSQL.Parser.Nodes
{
    public class FromTableNode : FromNode
    {
        public FromTableNode(string database, string schema, string tableOrView, string alias)
            : base(alias)
        {
            Database = database;
            Schema = schema;
            TableOrView = tableOrView;
            Id = $"{nameof(FromTableNode)}{database}{schema}{tableOrView}{Alias}";
        }

        public string Database { get; set; }

        public string Schema { get; }

        public string TableOrView { get; }

        public override string Id { get; }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {

            return $"from {Database}.{Schema}.{TableOrView} {Alias}";
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