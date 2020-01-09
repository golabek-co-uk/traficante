using System;
using System.Linq;

namespace Traficante.TSQL.Parser.Nodes
{
    public class SelectNode : Node
    {
        public SelectNode(FieldNode[] fields, bool? returnsSingleRow = null)
        {
            Fields = fields;
            ReturnsSingleRow = returnsSingleRow;
            var fieldsId = fields.Length == 0 ? string.Empty : fields.Select(f => f.Id).Aggregate((a, b) => a + b);
            Id = $"{nameof(SelectNode)}{fieldsId}";
        }

        public FieldNode[] Fields { get; }

        public override Type ReturnType { get; }

        public override string Id { get; }
        public bool? ReturnsSingleRow { get; set; }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            var fieldsTxt = Fields.Length == 0
                ? string.Empty
                : Fields.Select(FieldToString).Aggregate((a, b) => $"{a}, {b}");
            return $"select {fieldsTxt}";
        }

        private string FieldToString(FieldNode node)
        {
            return string.IsNullOrEmpty(node.FieldName) ? node.Expression.ToString() : node.FieldName;
        }
    }
}