namespace Traficante.TSQL.Parser.Nodes
{
    public class FromFunctionNode : FromNode
    {
        public FromFunctionNode(string database, string schema, string method, ArgsListNode parameters, string alias)
    : base(alias)
        {
            Database = database;
            Schema = schema;
            Method = method;
            MethodParameters = parameters;
            var paramsId = parameters.Id;
            Id = $"{nameof(FromFunctionNode)}{database}{schema}{method}{paramsId}{Alias}";
        }

        public string Database { get; set; }

        public string Schema { get; }

        public string Method { get; }

        public ArgsListNode MethodParameters { get; }

        public override string Id { get; }
        
        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {

            return $"from {Database}.{Schema}.{Method}({MethodParameters.Id}) {Alias}";
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is FromFunctionNode node)
                return node.Id == Id;

            return base.Equals(obj);
        }
    }

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