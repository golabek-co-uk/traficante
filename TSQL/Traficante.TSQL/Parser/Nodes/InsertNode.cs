using System;
using System.Linq;
using Traficante.TSQL.Evaluator.Visitors;

namespace Traficante.TSQL.Parser.Nodes
{
    public class InsertNode : Node
    {
        public InsertNode(TableNode table, FieldNode[] fields, Node[] values, SelectNode selectNode)
        {
            Values = values;
            Fields = fields;
        }
        public TableNode Table { get; }
        public FieldNode[] Fields { get; set; }
        public Node[] Values { get; }
        public SelectNode Select { get; }
        public override string Id { get; }
        public override Type ReturnType { get; }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            var fieldsTxt = Fields.Length == 0
                ? string.Empty
                : Fields.Select(FieldToString).Aggregate((a, b) => $"{a}, {b}");
            return $"insert ({fieldsTxt})";
        }

        private string FieldToString(FieldNode node)
        {
            return string.IsNullOrEmpty(node.FieldName) ? node.Expression.ToString() : node.FieldName;
        }
    }
}