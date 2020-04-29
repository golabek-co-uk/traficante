using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using Traficante.TSQL.Evaluator.Utils;
using Traficante.TSQL.Parser;
using Traficante.TSQL.Parser.Nodes;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq;
using Traficante.TSQL.Evaluator.Helpers;

namespace Traficante.TSQL.Evaluator.Visitors
{
    public class RunQueryTraverseVisitor : IExpressionVisitor
    {
        private readonly RunQueryVisitor _visitor;
        private readonly CancellationToken _cancellationToken;

        public RunQueryTraverseVisitor(RunQueryVisitor visitor, CancellationToken cancellationToken)
        {
            this._visitor = visitor ?? throw new ArgumentNullException(nameof(visitor));
            this._cancellationToken = cancellationToken;
        }

        public void Visit(SelectNode node)
        {
            SetQueryPart(QueryPart.Select);

            if (_visitor.CurrentQuery != null && _visitor.CurrentQuery.HasFromClosure())
            {
                Expression sequence = _visitor.Nodes.Peek();
                this._visitor.ScopedParamters.Push(Expression.Parameter(typeof(int), "item_i"));
                this._visitor.ScopedParamters.Push(Expression.Parameter(sequence.GetElementType(), "item_" + sequence.GetElementType().Name));
            }

            node.Top?.Accept(this);
            foreach (var field in node.Fields)
                field.Accept(this);
            node.Accept(_visitor);
        }
        public void Visit(StringNode node)
        {
            node.Accept(_visitor);
        }

        public void Visit(IntegerNode node)
        {
            node.Accept(_visitor);
        }

        public void Visit(BooleanNode node)
        {
            node.Accept(_visitor);
        }

        public void Visit(WordNode node)
        {
            node.Accept(_visitor);
        }

        public void Visit(ContainsNode node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);
            node.Accept(_visitor);
        }

        public void Visit(FunctionNode node)
        {
            if (this._visitor.CurrentQuery != null && this._visitor.CurrentQuery.HasFromClosure())
            {
                ParameterExpression item = _visitor.ScopedParamters.Peek();
                if (node.IsAggregateMethod)
                {
                    if (item.Type.IsGrouping())
                    {
                        var itemInGroup = Expression.Parameter(item.Type.GetGroupingElementType(), "itemInGroup_" + item.Type.GetGroupingElementType().Name);
                        this._visitor.ScopedParamters.Push(itemInGroup);
                    }
                    else
                    {
                        this._visitor.ScopedParamters.Push(item);
                    }
                }
            }

            node.Arguments.Accept(this);
            node.Accept(_visitor);
        }

        public void Visit(IsNullNode node)
        {
            node.Expression.Accept(this);
            node.Accept(_visitor);
        }

        public void Visit(AccessFieldNode node)
        {
            node.Accept(_visitor);
        }

        public void Visit(AllColumnsNode node)
        {
            node.Accept(_visitor);
        }

        public void Visit(IdentifierNode node)
        {
            node.Accept(_visitor);
        }

        public void Visit(AccessArrayFieldNode node)
        {
            node.Accept(_visitor);
        }

        public void Visit(AccessObjectKeyNode node)
        {
            node.Accept(_visitor);
        }

        public void Visit(PropertyValueNode node)
        {
            node.Accept(_visitor);
        }

        public void Visit(VariableNode node)
        {
            node.Accept(_visitor);
        }

        public void Visit(DeclareNode node)
        {
            node.Value?.Accept(this);
            node.Accept(_visitor);
        }

        public void Visit(SetNode node)
        {
            node.Value?.Accept(this);
            node.Accept(_visitor);
        }

        public void Visit(DotNode node)
        {
            //node.Root.Accept(this);
            //node.Expression.Accept(this);
            node.Accept(_visitor);
        }

        public virtual void Visit(WhereNode node)
        {
            Expression sequence = _visitor.Nodes.Peek();
            this._visitor.ScopedParamters.Push(Expression.Parameter(sequence.GetElementType(), "item_" + sequence.GetElementType().Name));
            this._visitor.ScopedParamters.Push(Expression.Parameter(typeof(int), "item_i"));

            SetQueryPart(QueryPart.Where);
            node.Expression.Accept(this);
            node.Accept(_visitor);
        }

        public void Visit(GroupByNode node)
        {
            SetQueryPart(QueryPart.GroupBy);

            Expression sequence = _visitor.Nodes.Peek();
            this._visitor.ScopedParamters.Push(Expression.Parameter(sequence.GetElementType(), "item_" + sequence.GetElementType().Name));

            foreach (var field in node.Fields)
                field.Accept(this);

            node.Accept(_visitor);
            node.Having?.Accept(this);
        }

        public void Visit(HavingNode node)
        {
            SetQueryPart(QueryPart.Having);

            Expression sequence = _visitor.Nodes.Peek();
            this._visitor.ScopedParamters.Push(Expression.Parameter(sequence.GetElementType(), "item_" + sequence.GetElementType().Name));


            node.Expression.Accept(this);
            node.Accept(_visitor);
        }

        public void Visit(SkipNode node)
        {
            node.Accept(_visitor);
        }

        public void Visit(TakeNode node)
        {
            node.Accept(_visitor);
        }

        public void Visit(TopNode node)
        {
            node.Accept(_visitor);
        }

        public void Visit(FromFunctionNode node)
        {
            SetQueryPart(QueryPart.From);
            //node.Function.Accept(this);
            node.Function.Arguments.Accept(this);
            node.Accept(_visitor);
        }

        public void Visit(FromTableNode node)
        {
            SetQueryPart(QueryPart.From);
            node.Accept(_visitor);
        }

        public void Visit(InMemoryTableFromNode node)
        {
            SetQueryPart(QueryPart.From);
            node.Accept(_visitor);
        }

        public void Visit(JoinNode node)
        {
            SetQueryPart(QueryPart.From);
            var joins = new Stack<JoinNode>();

            var join = node;
            while (join != null)
            {
                joins.Push(join);
                join = join.Source as JoinNode;
            }

            bool isFirstJoin = true;

            while (joins.Count > 0)
            {
                join = joins.Pop();
                if (isFirstJoin)
                {
                    join.Source.Accept(this);
                    isFirstJoin = false;
                }

                var sourceSequence = this._visitor.Nodes.Peek();
                this._visitor.ScopedParamters.Push(Expression.Parameter(sourceSequence.GetElementType(), "item_" + sourceSequence.GetElementType().Name));

                join.With.Accept(this);
                var withSequence = this._visitor.Nodes.Peek();
                this._visitor.ScopedParamters.Push(Expression.Parameter(withSequence.GetElementType(), "item_" + withSequence.GetElementType().Name));



                if (join.Expression is EqualityNode equalityNode)
                {
                    if (equalityNode.Left is BinaryNode == false || equalityNode.Right is BinaryNode == false)
                    {
                        bool canDoGroupJoin = true;
                        _visitor.AccessedFields.Clear();
                        equalityNode.Left.Accept(this);
                        if (_visitor.AccessedFields.GroupBy(x => x.Parameter).Count() > 1)
                            canDoGroupJoin = false;

                        _visitor.AccessedFields.Clear();
                        equalityNode.Right.Accept(this);
                        if (_visitor.AccessedFields.GroupBy(x => x.Parameter).Count() > 1)
                            canDoGroupJoin = false;

                        if (canDoGroupJoin)
                        {
                            join.JoinOperator = JoinOperator.Hash;
                            join.Accept(_visitor);
                        }
                        else
                        {
                            //remove left
                           _visitor.Nodes.Pop();
                            //remove rigth
                           _visitor.Nodes.Pop();

                            join.JoinOperator = JoinOperator.Loop;
                            join.Expression.Accept(this);
                            join.Accept(_visitor);
                        }
                    }
                    else
                    {
                        join.JoinOperator = JoinOperator.Loop;
                        join.Expression.Accept(this);
                        join.Accept(_visitor);
                    }
                }
                else
                {
                    join.JoinOperator = JoinOperator.Loop;
                    join.Expression.Accept(this);
                    join.Accept(_visitor);
                }
            }
        }

        public void Visit(ExpressionFromNode node)
        {
            SetQueryPart(QueryPart.From);
            node.Expression.Accept(this);
            node.Accept(_visitor);
        }

        public void Visit(IntoNode node)
        {
            node.Accept(_visitor);
        }

        public void Visit(QueryScope node)
        {
            node.Accept(_visitor);
        }

        public void Visit(QueryNode node)
        {
            Query queryState = new Query();
            queryState.QueryNode = node;
            _visitor.SetQuery(queryState);

            node.From?.Accept(this);
            node.Where?.Accept(this);

            node.GroupBy?.Accept(this);
            
            node.Skip?.Accept(this);
            node.Take?.Accept(this);

            node.OrderBy?.Accept(this);

            node.Select.Accept(this);
            node.Accept(_visitor);

            SetQueryPart(QueryPart.None);
        }

        public void Visit(OrNode node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);
            node.Accept(_visitor);
        }

        public void Visit(HyphenNode node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);
            node.Accept(_visitor);
        }

        public void Visit(AndNode node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);
            node.Accept(_visitor);
        }

        public void Visit(EqualityNode node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);
            node.Accept(_visitor);
        }

        public void Visit(GreaterOrEqualNode node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);
            node.Accept(_visitor);
        }

        public void Visit(LessOrEqualNode node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);
            node.Accept(_visitor);
        }

        public void Visit(GreaterNode node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);
            node.Accept(_visitor);
        }

        public void Visit(LessNode node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);
            node.Accept(_visitor);
        }

        public void Visit(DiffNode node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);
            node.Accept(_visitor);
        }

        public void Visit(NotNode node)
        {
            node.Expression.Accept(this);
            node.Accept(_visitor);
        }

        public void Visit(LikeNode node)
        {
            Visit(new FunctionNode(nameof(Operators.Like),
                new ArgsListNode(new[] { node.Left, node.Right }),
                new string[0],
                new Traficante.TSQL.Schema.Managers.MethodInfo { FunctionMethod = typeof(Operators).GetMethod(nameof(Operators.Like)) }));

            //node.Left.Accept(this);
            //node.Right.Accept(this);
            //node.Accept(_visitor);
        }

        public void Visit(RLikeNode node)
        {
            Visit(new FunctionNode(nameof(Operators.RLike),
                new ArgsListNode(new[] { node.Left, node.Right }),
                new string[0],
                new Traficante.TSQL.Schema.Managers.MethodInfo { FunctionMethod = typeof(Operators).GetMethod(nameof(Operators.RLike)) }));

            //node.Left.Accept(this);
            //node.Right.Accept(this);
            //node.Accept(_visitor);
        }

        public void Visit(InNode node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);
            node.Accept(_visitor);
        }

        public void Visit(FieldNode node)
        {
            _visitor.SetFieldNode(node);

            if (this._visitor.CurrentQuery != null && this._visitor.CurrentQuery.HasFromClosure())
            {
                ParameterExpression item = _visitor.ScopedParamters.Peek();
                if (item.Type.IsGrouping())
                {
                    var fields = item.GetFields(new[] { node.Expression.ToString() });
                    if (fields.Count == 1)
                    {
                        node.ChangeReturnType(fields.First().Type);
                        Visit(new IdentifierNode(node.Expression.ToString()));
                        node.Accept(_visitor);
                        _visitor.SetFieldNode(null);
                        return;
                    }
                }
            }

            node.Expression.Accept(this);
            node.Accept(_visitor);
            _visitor.SetFieldNode(null);
        }

        public void Visit(FieldOrderedNode node)
        {
            _visitor.SetFieldNode(node);
            node.Expression.Accept(this);
            node.Accept(_visitor);
            _visitor.SetFieldNode(null);
        }

        public void Visit(ArgsListNode node)
        {
            foreach (var item in node.Args)
                item.Accept(this);
            node.Accept(_visitor);
        }

        public void Visit(DecimalNode node)
        {
            node.Accept(_visitor);
        }

        public void Visit(Node node)
        {
            node.Accept(_visitor);
        }

        public void Visit(DescNode node)
        {
            node.Accept(_visitor);
        }

        public void Visit(StarNode node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);
            node.Accept(_visitor);
        }

        public void Visit(FSlashNode node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);
            node.Accept(_visitor);
        }

        public void Visit(ModuloNode node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);
            node.Accept(_visitor);
        }

        public void Visit(AddNode node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);
            node.Accept(_visitor);
        }

        public void Visit(RootNode node)
        {
            node.Expression.Accept(this);
            node.Accept(_visitor);
        }

        public void Visit(SingleSetNode node)
        {
            node.Query.Accept(this);
            node.Accept(_visitor);
        }

        public void Visit(UnionNode node)
        {
            TraverseSetOperator(node);
        }

        public void Visit(UnionAllNode node)
        {
            TraverseSetOperator(node);
        }

        public void Visit(ExceptNode node)
        {
            TraverseSetOperator(node);
        }

        public void Visit(IntersectNode node)
        {
            TraverseSetOperator(node);
        }

        public void Visit(PutTrueNode node)
        {
            node.Accept(_visitor);
        }

        public void Visit(MultiStatementNode node)
        {
            foreach (var cNode in node.Nodes)
                cNode.Accept(this);
            node.Accept(_visitor);
        }

        public void Visit(CteExpressionNode node)
        {
            foreach (var exp in node.InnerExpression) exp.Accept(this);
            node.OuterExpression.Accept(this);
            node.Accept(_visitor);
        }

        public void Visit(CteInnerExpressionNode node)
        {
            node.Value.Accept(this);
            node.Accept(_visitor);
        }


        public void Visit(FromNode node)
        {
            node.Accept(_visitor);
        }

        public void Visit(OrderByNode node)
        {
            Expression sequence = _visitor.Nodes.Peek();
            this._visitor.ScopedParamters.Push(Expression.Parameter(sequence.GetElementType(), "item_" + sequence.GetElementType().Name));

            foreach (var field in node.Fields)
                field.Accept(this);

            node.Accept(_visitor);
        }

        public void Visit(CreateTableNode node)
        {
            node.Accept(_visitor);
        }

        public void Visit(StatementsArrayNode node)
        {
            foreach (var statement in node.Statements)
                statement.Accept(this);

            node.Accept(_visitor);
        }

        public void Visit(StatementNode node)
        {
            node.Node.Accept(this);
            node.Accept(_visitor);
        }

        public void Visit(CaseNode node)
        {
            node.Else.Accept(this);

            for (int i = node.WhenThenPairs.Length - 1; i >= 0; --i)
            {
                node.WhenThenPairs[i].When.Accept(this);
                node.WhenThenPairs[i].Then.Accept(this);
            }

            node.Accept(_visitor);
        }

        public void Visit(TypeNode node)
        {
            node.Accept(_visitor);
        }

        public void Visit(ExecuteNode node)
        {
            node.FunctionToRun?.Accept(this);
            node.Accept(_visitor);
        }

        private void TraverseSetOperator(SetOperatorNode node)
        {
            if (node.Right is SetOperatorNode)
            {
                var nodes = new Stack<SetOperatorNode>();
                nodes.Push(node);

                node.Left.Accept(this);

                while (nodes.Count > 0)
                {
                    var current = nodes.Pop();

                    if (current.Right is SetOperatorNode operatorNode)
                    {
                        nodes.Push(operatorNode);

                        operatorNode.Left.Accept(this);

                        current.Accept(_visitor);
                    }
                    else
                    {
                        current.Right.Accept(this);

                        current.Accept(_visitor);
                    }
                }
            }
            else
            {
                node.Left.Accept(this);

                node.Right.Accept(this);

                node.Accept(_visitor);
            }
        }

        public void SetQueryPart(QueryPart part)
        {
            _visitor.SetQueryPart(part);
        }

    }
}