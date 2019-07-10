using Traficante.TSQL.Evaluator.TemporarySchemas;
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
            items.SchemaProvider = new TransitionSchemaProvider(items.SchemaProvider);

            var queryTree = items.RawQueryTree;

            var metadataInferer = new BuildMetadataAndInferTypeVisitor(items.SchemaProvider);
            var metadataInfererTraverser = new BuildMetadataAndInferTypeTraverseVisitor(metadataInferer);

            queryTree.Accept(metadataInfererTraverser);

            queryTree = metadataInferer.Root;

            var rewriter = new RewriteQueryVisitor();
            var rewriteTraverser = new RewriteQueryTraverseVisitor(rewriter, new ScopeWalker(metadataInfererTraverser.Scope));

            queryTree.Accept(rewriteTraverser);

            queryTree = rewriter.RootScript;

            var csharpRewriter = new ToCSharpStreamRewriteVisitor(items.SchemaProvider, metadataInferer.SetOperatorFieldPositions, metadataInferer.InferredColumns);
            var csharpRewriteTraverser = new ToCSharpStreamRewriteTraverseVisitor(csharpRewriter, new ScopeWalker(metadataInfererTraverser.Scope));

            queryTree.Accept(csharpRewriteTraverser);

            items.Stream = csharpRewriter.ResultStream;
            items.Columns = csharpRewriter.ResultColumns.Last().Value;
            items.ColumnsTypes = csharpRewriter.ResultColumnsTypes.Last().Value;

            Successor?.Build(items);
        }
    }
}