using Traficante.TSQL.Parser.Nodes;
using Traficante.TSQL.Schema.DataSources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Traficante.TSQL.Evaluator.Visitors
{
    public static class QueryableObjectResolver
    {
        //public static IQueryable<IObjectResolver> Select(
        //    this IQueryable<IObjectResolver> source, 
        //    IEnumerable<string> names,
        //    IEnumerable<object> values)
        //{
        //    source.Select<IObjectResolver, IObjectResolver>(x => new DictionaryResolver(fields, values));

        //    return source.Select((x) =>
        //    {
        //        var obj = new DictionaryResolver();
        //        using (IEnumerator<FieldNode> enumeratorFields = fields.GetEnumerator())
        //        using (IEnumerator<object> enumeratorValues = values.GetEnumerator())
        //        {
        //            while (enumeratorFields.MoveNext() && enumeratorValues.MoveNext())
        //            {
        //                obj[enumeratorFields.Current.FieldName] = enumeratorValues.Current;
        //            }
        //        }
        //        return (IObjectResolver)obj;
        //    });
        //}
    }
}
