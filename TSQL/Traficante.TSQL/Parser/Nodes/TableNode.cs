using System;
using Traficante.TSQL.Evaluator.Visitors;

namespace Traficante.TSQL.Parser.Nodes
{
    public class TableNode : Node
    {
        public TableNode(string tableOrView, string[] path)
                    : base()
        {
            Path = path;
            TableOrView = tableOrView;
            Id = $"{nameof(TableNode)}.{string.Join(".", Path)}{(Path.Length > 0 ? "." : "")}{tableOrView}";
        }


        public string[] Path { get; }

        public string TableOrView { get; }

        public override string Id { get; }

        public override Type ReturnType => typeof(void);

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"{string.Join(".", Path)}{(Path.Length > 0 ? "." : "")}{TableOrView}";
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