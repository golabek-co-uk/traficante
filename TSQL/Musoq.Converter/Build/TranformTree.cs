using Musoq.Evaluator.TemporarySchemas;
using Musoq.Evaluator.Utils;
using Musoq.Evaluator.Visitors;
using System.Linq;

namespace Musoq.Converter.Build
{
    public class TransformTree : BuildChain
    {
        public TransformTree(BuildChain successor) 
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

            var csharpRewriter = new ToCSharpRewriteTreeVisitor(metadataInferer.Assemblies, metadataInferer.SetOperatorFieldPositions, metadataInferer.InferredColumns);
            var csharpRewriteTraverser = new ToCSharpRewriteTreeTraverseVisitor(csharpRewriter, new ScopeWalker(metadataInfererTraverser.Scope));

            queryTree.Accept(csharpRewriteTraverser);

            items.TransformedQueryTree = queryTree;
            items.Compilation = csharpRewriter.Compilation;
            items.AccessToClassPath = csharpRewriter.AccessToClassPath;

            Successor?.Build(items);
        }
    }

    public class TransformToStreamTree : BuildChain
    {
        public TransformToStreamTree(BuildChain successor)
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

            items.Stream = csharpRewriter.Stream;
            items.Columns = csharpRewriter.Columns.First().Value;
            items.ColumnsTypes = csharpRewriter.ColumnsTypes.First().Value;

            Successor?.Build(items);
        }
    }
}