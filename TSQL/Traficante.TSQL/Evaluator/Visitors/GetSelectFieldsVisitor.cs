using System.Collections.Generic;
using System.Linq;
using Traficante.TSQL.Parser;
using Traficante.TSQL.Parser.Nodes;
using Traficante.TSQL.Schema;
using Traficante.TSQL.Schema.DataSources;

namespace Traficante.TSQL.Evaluator.Visitors
{
    public class GetSelectFieldsVisitor : IQueryPartAwareExpressionVisitor
    {
        private readonly List<Schema.DataSources.Column> _collectedFieldNames = new List<Schema.DataSources.Column>();
        private QueryPart _queryPart;

        public Schema.DataSources.Column[] CollectedFieldNames => _collectedFieldNames.ToArray();

        public void SetQueryPart(QueryPart part)
        {
            _queryPart = part;
        }

        public void Visit(Node node)
        {
        }

        public void Visit(DescNode node)
        {
        }

        public void Visit(StarNode node)
        {
        }

        public void Visit(FSlashNode node)
        {
        }

        public void Visit(ModuloNode node)
        {
        }

        public void Visit(AddNode node)
        {
        }

        public void Visit(HyphenNode node)
        {
        }

        public void Visit(AndNode node)
        {
        }

        public void Visit(OrNode node)
        {
        }

        public void Visit(EqualityNode node)
        {
        }

        public void Visit(GreaterOrEqualNode node)
        {
        }

        public void Visit(LessOrEqualNode node)
        {
        }

        public void Visit(GreaterNode node)
        {
        }

        public void Visit(LessNode node)
        {
        }

        public void Visit(DiffNode node)
        {
        }

        public void Visit(NotNode node)
        {
        }

        public void Visit(LikeNode node)
        {
        }

        public void Visit(RLikeNode node)
        {
        }

        public void Visit(InNode node)
        {
        }

        public void Visit(FieldNode node)
        {
            if (_queryPart == QueryPart.Select && _collectedFieldNames.All(field => field.ColumnName != node.FieldName))
                _collectedFieldNames.Add(new Column(node.FieldName, _collectedFieldNames.Count, node.ReturnType));
        }

        public void Visit(FieldOrderedNode node)
        {
        }

        public void Visit(StringNode node)
        {
        }

        public void Visit(DecimalNode node)
        {
        }

        public void Visit(IntegerNode node)
        {
        }

        public void Visit(BooleanNode node)
        {
        }

        public void Visit(WordNode node)
        {
        }

        public void Visit(ContainsNode node)
        {
        }

        public void Visit(FunctionNode node)
        {
        }

        public void Visit(IsNullNode node)
        {
        }

        public void Visit(AccessColumnNode node)
        {
        }

        public void Visit(AllColumnsNode node)
        {
        }

        public void Visit(IdentifierNode node)
        {
        }

        public void Visit(AccessObjectArrayNode node)
        {
        }

        public void Visit(AccessObjectKeyNode node)
        {
        }

        public void Visit(PropertyValueNode node)
        {
        }

        public void Visit(VariableNode node)
        {
        }

        public virtual void Visit(DeclareNode node)
        {
        }

        public virtual void Visit(SetNode node)
        {
        }

        public void Visit(DotNode node)
        {
        }

        public void Visit(ArgsListNode node)
        {
        }

        public void Visit(SelectNode node)
        {
        }

        public void Visit(WhereNode node)
        {
        }

        public void Visit(GroupByNode node)
        {
        }

        public void Visit(HavingNode node)
        {
        }

        public void Visit(SkipNode node)
        {
        }

        public void Visit(TakeNode node)
        {
        }


        public void Visit(FromFunctionNode node)
        {
        }

        public void Visit(FromTableNode node)
        {
        }

        public void Visit(InMemoryTableFromNode node)
        {
        }

        public void Visit(JoinFromNode node)
        {
        }

        public void Visit(ExpressionFromNode node)
        {
        }

        public void Visit(IntoNode node)
        {
        }

        public void Visit(QueryScope node)
        {
        }

        public void Visit(QueryNode node)
        {
        }

        public void Visit(RootNode node)
        {
        }

        public void Visit(SingleSetNode node)
        {
        }

        public void Visit(UnionNode node)
        {
        }

        public void Visit(UnionAllNode node)
        {
        }

        public void Visit(ExceptNode node)
        {
        }

        public void Visit(IntersectNode node)
        {
        }

        public void Visit(PutTrueNode node)
        {
        }

        public void Visit(MultiStatementNode node)
        {
        }

        public void Visit(CteExpressionNode node)
        {
        }

        public void Visit(CteInnerExpressionNode node)
        {
        }

        public void Visit(JoinsNode node)
        {
        }

        public void Visit(JoinNode node)
        {
        }

        public void Visit(OrderByNode node)
        {
        }

        public void Visit(CreateTableNode node)
        {
        }
    
        public void Visit(StatementsArrayNode node)
        {
        }

        public void Visit(StatementNode node)
        {
        }

        public void Visit(CaseNode node)
        {
        }

        public void Visit(TypeNode node)
        {
        }

        public void Visit(ExecuteNode node)
        {
        }
    }
}