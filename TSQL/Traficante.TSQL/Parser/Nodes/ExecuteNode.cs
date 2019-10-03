using System;

namespace Traficante.TSQL.Parser.Nodes
{
    public class ExecuteNode : Node
    {
        public ExecuteNode(string database, string schema, string method, ArgsListNode parameters, VariableNode variableToSet)
        {
            Database = database;
            Schema = schema;
            Method = method;
            MethodParameters = parameters;
            var paramsId = parameters.Id;
            Id = $"{nameof(ExecuteNode)}{database}{schema}{method}{paramsId}";
        }

        public string Database { get; set; }

        public string Schema { get; }

        public string Method { get; }

        public ArgsListNode MethodParameters { get; }

        public VariableNode VariableToSet { get; }

        public override string Id { get; }

        public override Type ReturnType => typeof(void);

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {

            return $"execute {Database}.{Schema}.{Method} {MethodParameters.Id}";
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