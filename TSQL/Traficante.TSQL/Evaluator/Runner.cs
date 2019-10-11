using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Traficante.TSQL.Converter.Build;
using Traficante.TSQL.Evaluator;
using Traficante.TSQL.Evaluator.Tables;
using Traficante.TSQL.Schema;
using Traficante.TSQL.Schema.DataSources;
using Column = Traficante.TSQL.Evaluator.Tables.Column;

namespace Traficante.TSQL.Converter
{
    public class Runner
    {
        public object Run(string script, IEngine engine)
        {
            var items = new BuildItems
            {
                RawQuery = script,
                Engine = engine,
            };

            BuildChain chain =
                new CreateTree(
                    new TransformToQueryableStreamTree(null));
            chain.Build(items);

            return items.Result;
        }

        public Table RunAndReturnTable(string script, IEngine engine)
        {
            

            object result = Run(script, engine);
            if (result is System.Collections.IEnumerable enumerableResult)
            {
                var itemType = result.GetType().GenericTypeArguments.FirstOrDefault();

                List<Column> columns2 = new List<Column>();
                int index = 0;
                foreach (var field in itemType.GetFields())
                {
                    columns2.Add(new Column(field.Name, field.FieldType, index));
                    index++;
                }

                Evaluator.Tables.Table t = new Evaluator.Tables.Table("entities", columns2.ToArray());
                foreach (var row in enumerableResult)
                {
                    object[] values = new object[columns2.Count];
                    for (int i = 0; i < columns2.Count; i++)
                    {
                        values[i] = itemType.GetField(columns2[i].ColumnName).GetValue(row);
                    }
                    ObjectsRow row2 = new ObjectsRow(values);
                    t.Add(row2);
                }
                return t;
            }
            return null;
        }
    }

}