using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Traficante.TSQL.Evaluator.Visitors;
using Traficante.TSQL.Parser.Lexing;

namespace Traficante.TSQL.Converter
{
    public class Runner
    {
        public object Run(string script, TSQLEngine engine, CancellationToken cancellationToken)
        {
            try
            {
                var lexer = new Lexer(script, true);
                var parser = new Parser.Parser(lexer);
                var queryTree = parser.ComposeAll();

                var prepareQuery = new PrepareQueryVisitor(engine, cancellationToken);
                var prepareQueryTraverser = new PrepareQueryTraverseVisitor(prepareQuery, cancellationToken);
                queryTree.Accept(prepareQueryTraverser);
                queryTree = prepareQuery.Root;

                var requestData = new RequestDataVisitor(engine, cancellationToken);
                var requestDataTraverser = new RequestDataTraverseVisitor(requestData, cancellationToken);
                queryTree.Accept(requestDataTraverser);
                queryTree = requestData.Root;

                var runQuery = new RunQueryVisitor(engine, cancellationToken);
                var csharpRewriteTraverser = new RunQueryTraverseVisitor(runQuery, cancellationToken);
                queryTree.Accept(csharpRewriteTraverser);
                return runQuery.Result;
            } catch(AggregateException ex)
            {
                throw ex.InnerException;
            }
        }
    }

}