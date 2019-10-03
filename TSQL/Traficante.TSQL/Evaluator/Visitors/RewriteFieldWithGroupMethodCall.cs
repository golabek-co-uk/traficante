﻿using System.Collections.Generic;
using System.Linq;
using Traficante.TSQL.Evaluator.Helpers;
using Traficante.TSQL.Parser;
using Traficante.TSQL.Parser.Nodes;
using Traficante.TSQL.Parser.Tokens;
using Traficante.TSQL.Plugins;

namespace Traficante.TSQL.Evaluator.Visitors
{
    public class RewriteFieldWithGroupMethodCall : CloneQueryVisitor
    {
        private readonly FieldNode[] _fields;

        public RewriteFieldWithGroupMethodCall(FieldNode[] fields)
        {
            _fields = fields;
        }

        public FieldNode Expression { get; private set; }

        public override void Visit(FieldNode node)
        {
            base.Visit(node);
            Expression = Nodes.Pop() as FieldNode;
        }

        public override void Visit(AccessColumnNode node)
        {
            Nodes.Push(new AccessColumnNode(NamingHelper.ToColumnName(node.Alias, node.Name), string.Empty,
                node.ReturnType, TextSpan.Empty));
        }

        public override void Visit(DotNode node)
        {
            if (!(node.Root is DotNode) && node.Root is AccessColumnNode column)
            {
                Nodes.Pop();
                Nodes.Pop();

                var name = $"{NamingHelper.ToColumnName(column.Alias, column.Name)}.{node.Expression.ToString()}";
                Nodes.Push(new AccessColumnNode(name, string.Empty, node.ReturnType, TextSpan.Empty));
                return;
            }

            base.Visit(node);
        }

        public override void Visit(FunctionNode node)
        {
            //if (node.IsAggregateMethod)
            //{
            //    Nodes.Pop();

            //    var wordNode = node.Arguments.Args[0] as WordNode;
            //    var accessGroup = new AccessColumnNode("none", string.Empty, typeof(Group), TextSpan.Empty);
            //    var args = new List<Node> {accessGroup, wordNode};
            //    args.AddRange(node.Arguments.Args.Skip(1));
            //    var extractFromGroup = new AccessMethodNode(
            //        new FunctionToken(node.Method.Name, TextSpan.Empty), 
            //        new ArgsListNode(args.ToArray()), node.ExtraAggregateArguments, node.Method, node.Alias);
            //    Nodes.Push(extractFromGroup);
            //}
            //else 
            if (_fields.Select(f => f.Expression.ToString()).Contains(node.ToString()))
            {
                Nodes.Push(new AccessColumnNode(node.ToString(), string.Empty, node.ReturnType, TextSpan.Empty));
            }
            else
            {
                base.Visit(node);
            }
        }

        public override void Visit(AccessCallChainNode node)
        {
            Nodes.Push(new AccessColumnNode(node.ToString(), string.Empty, node.ReturnType, TextSpan.Empty));
        }
    }
}