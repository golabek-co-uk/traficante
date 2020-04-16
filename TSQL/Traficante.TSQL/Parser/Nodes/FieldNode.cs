using System;
using Traficante.TSQL.Evaluator.Visitors;

namespace Traficante.TSQL.Parser.Nodes
{
    public class FieldNode : Node
    {
        private readonly string _fieldName;
        private Type _type;

        public FieldNode(Node expression, int fieldOrder, string fieldName)
        {
            Expression = expression;
            FieldOrder = fieldOrder;
            _fieldName = fieldName;
            Id = $"{nameof(FieldNode)}{expression.Id}";
        }

        public Node Expression { get; }

        public int FieldOrder { get; }

        public string FieldName => string.IsNullOrEmpty(_fieldName) ? Expression.ToString() : _fieldName;

        public override Type ReturnType => _type ?? Expression.ReturnType;

        public override string Id { get; }

        public void ChangeReturnType(Type type)
        {
            this._type = type;
        }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(_fieldName)
                ? $"{Expression.ToString()}"
                : $"{Expression.ToString()} as {_fieldName}";
        }
    }
}