﻿using System;
using Traficante.TSQL.Evaluator.Visitors;

namespace Traficante.TSQL.Parser.Nodes
{
    public class HavingNode : Node
    {
        public HavingNode(Node expression)
        {
            Expression = expression;
            Id = $"{nameof(HavingNode)}{expression.Id}";
        }

        public Node Expression { get; }

        public override Type ReturnType { get; }

        public override string Id { get; }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"HAVING {Expression.ToString()}";
        }
    }
}