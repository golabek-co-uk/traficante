using Traficante.TSQL.Evaluator.Utils;
using Traficante.TSQL.Evaluator.Visitors;
using System.Linq;

namespace Traficante.TSQL.Converter.Build
{
    public class TransformToQueryableStreamTree : BuildChain
    {
        public TransformToQueryableStreamTree(BuildChain successor)
            : base(successor)
        {
        }

        public override void Build(BuildItems items)
        {
            var queryTree = items.RawQueryTree;

            var prepareQuery = new PrepareQueryVisitor(items.Engine);
            var prepareQueryTraverser = new PrepareQueryTraverseVisitor(prepareQuery);
            queryTree.Accept(prepareQueryTraverser);
            queryTree = prepareQuery.Root;

            var csharpRewriter = new RunQueryVisitor(items.Engine);
            var csharpRewriteTraverser = new RunTraverseVisitor(csharpRewriter);
            queryTree.Accept(csharpRewriteTraverser);
            items.Result = csharpRewriter.Result;

            Successor?.Build(items);
        }
    }
}