﻿using System;
using Traficante.TSQL.Evaluator.Visitors;

namespace Traficante.TSQL.Parser.Nodes
{
    public class AllColumnsNode : Node
    {
        public override Type ReturnType => typeof(object[]);

        public override string Id => $"{nameof(AllColumnsNode)}*";

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return "*";
        }
    }
}