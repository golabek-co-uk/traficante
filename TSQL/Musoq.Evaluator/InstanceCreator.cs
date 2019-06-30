using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Musoq.Converter.Build;
using Musoq.Evaluator;
using Musoq.Evaluator.Runtime;
using Musoq.Evaluator.Tables;
using Musoq.Schema;
using Musoq.Schema.DataSources;
using Column = Musoq.Evaluator.Tables.Column;

namespace Musoq.Converter
{
    public static class InstanceCreator
    {
        public static CompiledQuery CompileForExecution(string script, IDatabaseProvider schemaProvider)
        {
            var items = new BuildItems
            {
                SchemaProvider = schemaProvider,
                RawQuery = script
            };

            //RuntimeLibraries.CreateReferences();

            BuildChain chain =
                new CreateTree(
                    new TransformToQueryableStreamTree(null));
            chain.Build(items);

            return new CompiledQuery(CreateRunnableStream(items));
        }

        private static IRunnable CreateRunnableStream(BuildItems items)
        {
            var runnableStream = new RunnableStream();
            runnableStream.Provider = items.SchemaProvider;
            runnableStream.Stream = items.Stream;
            runnableStream.Columns = items.Columns;
            runnableStream.ColumnsTypes = items.ColumnsTypes;
            return runnableStream;
        }
    }

    public class RunnableStream : IRunnable
    {
        public IDatabaseProvider Provider { get; set; }
        public IQueryable<IObjectResolver> Stream { get; set; }
        public string[] Columns { get; set; }
        public Type[] ColumnsTypes { get; set; }
        

        public Table Run(CancellationToken token)
        {
            List<Column> columns2 = new List<Column>();
            for(int i = 0; i < Columns.Length; i++)
            {
                columns2.Add(new Column(Columns[i], ColumnsTypes[i], i));
            }
            
            Table t = new Table("entities", columns2.ToArray());
            foreach(var row in Stream.ToList())
            {
                object[] values = new object[columns2.Count];
                for (int i = 0; i < Columns.Length; i++)
                {
                    values[i] = row.GetValue(Columns[i]);
                }
                ObjectsRow row2 = new ObjectsRow(values);
                t.Add(row2);
            }
            return t;
        }
    }
}