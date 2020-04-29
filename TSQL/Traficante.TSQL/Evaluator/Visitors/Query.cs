using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Traficante.TSQL.Parser.Nodes;

namespace Traficante.TSQL.Evaluator.Visitors
{
    public class Query : IDisposable
    {
        public  QueryNode QueryNode { get; set; }

        public List<FieldNode> SelectedFieldsNodes = new List<FieldNode>();

        public bool HasFromClosure()
        {
            return QueryNode?.From != null;
        }

        public bool IsSingleRowResult()
        {
            if (QueryNode.From == null)
                return true;


            if (QueryNode.GroupBy == null)
            {
                var split = SplitBetweenAggreateAndNonAggreagate(QueryNode.Select.Fields);
                if (split.NotAggregateFields.Length == 0)
                    return true;
            }

            return false;
        }

        private (FieldNode[] AggregateFields, FieldNode[] NotAggregateFields) SplitBetweenAggreateAndNonAggreagate(FieldNode[] fieldsToSplit)
        {
            var aggregateFields = new List<FieldNode>();
            var notAggregateFields = new List<FieldNode>();

            foreach (var root in fieldsToSplit)
            {
                var subNodes = new Stack<Node>();

                subNodes.Push(root.Expression);
                bool hasAggregateMethod = false;
                while (subNodes.Count > 0)
                {
                    var subNode = subNodes.Pop();

                    if (subNode is FunctionNode aggregateMethod && aggregateMethod.IsAggregateMethod)
                    {
                        hasAggregateMethod = true;
                        break;
                    }
                    else
                    if (subNode is FunctionNode method)
                    {
                        foreach (var arg in method.Arguments.Args)
                            subNodes.Push(arg);
                    }
                    else if (subNode is BinaryNode binary)
                    {
                        subNodes.Push(binary.Left);
                        subNodes.Push(binary.Right);
                    }
                }
                if (hasAggregateMethod)
                    aggregateFields.Add(root);
                else
                    notAggregateFields.Add(root);
            }

            return (aggregateFields.ToArray(), notAggregateFields.ToArray());
        }


        public void Dispose()
        {
        }
    }
}
