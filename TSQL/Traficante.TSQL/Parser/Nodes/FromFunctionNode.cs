using Traficante.TSQL.Evaluator.Visitors;

namespace Traficante.TSQL.Parser.Nodes
{
    public class FromFunctionNode : FromNode
    {
        public FromFunctionNode(FunctionNode function, string alias)
    : base(alias)
        {
            Function = function;
            Id = $"{nameof(FromFunctionNode)}{Function.Id}";
        }

        public FunctionNode Function { get; set; }

        public override string Id { get; }
        
        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {

            return $"from {Function.ToString()} {Alias}";
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

}