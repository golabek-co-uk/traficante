using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Traficante.TSQL.Parser.Nodes;

namespace Traficante.TSQL.Evaluator.Visitors
{
    public class QueryState : IDisposable
    {
        public  QueryNode QueryNode { get; set; }

        public List<FieldNode> SelectedFieldsNodes = new List<FieldNode>();

        public ParameterExpression QueryItem = null;
        public ParameterExpression QueryItemIndex = Expression.Parameter(typeof(int), "item_i");
        public Dictionary<string, Expression> Alias2QueryItem = new Dictionary<string, Expression>();

        public ParameterExpression ItemInGroup = null;

        public ParameterExpression Query = null;

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

        public List<IDisposable> Disposables = new List<IDisposable>();

        public void Dispose()
        {
            foreach (var disposable in Disposables)
                disposable.Dispose();
        }
    }
}
