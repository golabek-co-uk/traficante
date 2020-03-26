using System;
using System.Collections.Generic;
using System.Linq;
using Traficante.TSQL.Evaluator.Visitors;
using Traficante.TSQL.Parser.Lexing;

namespace Traficante.TSQL.Converter
{
    public class Runner
    {
        public object Run(string script, TSQLEngine engine)
        {
            try
            {
                var lexer = new Lexer(script, true);
                var parser = new Parser.Parser(lexer);
                var queryTree = parser.ComposeAll();

                var prepareQuery = new PrepareQueryVisitor(engine);
                var prepareQueryTraverser = new PrepareQueryTraverseVisitor(prepareQuery);
                queryTree.Accept(prepareQueryTraverser);
                queryTree = prepareQuery.Root;

                var requestData = new RequestDataVisitor(engine);
                var requestDataTraverser = new RequestDataTraverseVisitor(requestData);
                queryTree.Accept(requestDataTraverser);
                queryTree = requestData.Root;

                var runQuery = new RunQueryVisitor(engine);
                var csharpRewriteTraverser = new RunQueryTraverseVisitor(runQuery);
                queryTree.Accept(csharpRewriteTraverser);
                return runQuery.Result;
            } catch(AggregateException ex)
            {
                throw ex.InnerException;
            }
        }

        public DataTable RunAndReturnTable(string script, TSQLEngine engine)
        {
            object result = Run(script, engine);
            if (result is System.Collections.IEnumerable enumerableResult)
            {
                var itemType = result.GetType().GenericTypeArguments.FirstOrDefault();

                List<DataColumn> columns2 = new List<DataColumn>();
                int index = 0;
                foreach (var field in itemType.GetFields())
                {
                    columns2.Add(new DataColumn(field.Name, field.FieldType, index));
                    index++;
                }

                DataTable t = new DataTable("entities", columns2.ToArray());
                foreach (var row in enumerableResult)
                {
                    object[] values = new object[columns2.Count];
                    for (int i = 0; i < columns2.Count; i++)
                    {
                        values[i] = itemType.GetField(columns2[i].ColumnName).GetValue(row);
                    }
                    DataRow row2 = new DataRow(values);
                    t.Add(row2);
                }
                return t;
            }
            return null;
        }
    }

}