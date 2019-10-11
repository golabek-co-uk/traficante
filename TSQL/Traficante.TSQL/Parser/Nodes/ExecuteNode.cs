using System;

namespace Traficante.TSQL.Parser.Nodes
{
    public class ExecuteNode : Node
    {
        public ExecuteNode(VariableNode variableToSet, FunctionNode functionToExecute)
        {
            FunctionToRun = functionToExecute;
            VariableToSet = variableToSet;
            Id = $"{nameof(ExecuteNode)}{functionToExecute?.Id}{functionToExecute?.Id}";
        }

        

        public VariableNode VariableToSet { get; }
        public FunctionNode FunctionToRun { get; }

        public override string Id { get; }

        public override Type ReturnType => typeof(void);

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            if (VariableToSet != null)
                return $"execute {VariableToSet.ToString()} = {FunctionToRun.ToString()}";
            else
                return $"execute {FunctionToRun.ToString()}";
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is ExecuteNode node)
                return node.Id == Id;

            return base.Equals(obj);
        }
    }

}