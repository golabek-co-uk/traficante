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

            var metadataInferer = new PrepareQueryVisitor(items.Engine);
            var metadataInfererTraverser = new PrepareQueryTraverseVisitor(metadataInferer);

            queryTree.Accept(metadataInfererTraverser);

            queryTree = metadataInferer.Root;

            var csharpRewriter = new ExecuteQueryVisitor(items.Engine);//, metadataInferer.SetOperatorFieldPositions);
            var csharpRewriteTraverser = new RunTraverseVisitor(csharpRewriter, new ScopeWalker(metadataInfererTraverser.Scope));

            queryTree.Accept(csharpRewriteTraverser);

            items.Result = csharpRewriter.Result;

            Successor?.Build(items);
        }
    }
}