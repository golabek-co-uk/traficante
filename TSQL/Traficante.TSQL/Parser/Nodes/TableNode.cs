using System;

namespace Traficante.TSQL.Parser.Nodes
{
    public class TableNode : Node
    {
        public TableNode(string tableOrView, string schema = null, string database = null, string server = null)
                    : base()
        {
            Database = database;
            Schema = schema;
            TableOrView = tableOrView;
            Id = $"{nameof(TableNode)}{server}{database}{schema}{tableOrView}";
        }

        public string Database { get; set; }

        public string Schema { get; }

        public string TableOrView { get; }

        public override string Id { get; }

        public override Type ReturnType => typeof(void);

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"{TableOrView}";
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is TableNode node)
                return node.Id == Id;

            return base.Equals(obj);
        }
    }

}