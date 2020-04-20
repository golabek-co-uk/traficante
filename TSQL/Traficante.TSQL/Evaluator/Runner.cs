using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Traficante.TSQL.Evaluator.Visitors;
using Traficante.TSQL.Parser;
using Traficante.TSQL.Parser.Lexing;
using Traficante.TSQL.Parser.Nodes;

namespace Traficante.TSQL.Converter
{
    public class Runner
    {
        private Lexer lexer;
        private Parser.Parser parser;
        private RootNode queryTree;
        private RunQueryVisitor runQuery;
        private RunQueryTraverseVisitor csharpRewriteTraverser;
        private RequestDataVisitor requestData;
        private RequestDataTraverseVisitor requestDataTraverser;
        private PrepareQueryTraverseVisitor prepareQueryTraverser;

        public object Run(string script, TSQLEngine engine, CancellationToken cancellationToken)
        {
            try
            {
                this.lexer = new Lexer(script, true);
                this.parser = new Parser.Parser(lexer);
                this.queryTree = parser.ComposeAll();

                var prepareQuery = new PrepareQueryVisitor(engine, cancellationToken);
                this.prepareQueryTraverser = new PrepareQueryTraverseVisitor(prepareQuery, cancellationToken);
                queryTree.Accept(prepareQueryTraverser);
                queryTree = prepareQuery.Root;

                this.requestData = new RequestDataVisitor(engine, cancellationToken);
                this.requestDataTraverser = new RequestDataTraverseVisitor(requestData, cancellationToken);
                queryTree.Accept(requestDataTraverser);
                queryTree = requestData.Root;

                this.runQuery = new RunQueryVisitor(engine, cancellationToken);
                this.csharpRewriteTraverser = new RunQueryTraverseVisitor(runQuery, cancellationToken);
                queryTree.Accept(csharpRewriteTraverser);
                return runQuery.Result;
            } catch(AggregateException ex)
            {
                throw ex.InnerException;
            }
        }
    }

}