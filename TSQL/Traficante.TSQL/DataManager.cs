using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Traficante.TSQL.Evaluator.Helpers;

namespace Traficante.TSQL
{
    public class DataManager
    {
        public List<TableResult> _tableData = new List<TableResult>();
        private readonly TSQLEngine _engine;

        public DataManager(TSQLEngine engine)
        {
            this._engine = engine;
        }
        
        public void StartRequestingTable(string name, string[] path, CancellationToken ct)
        {
            var tableData = new TableResult
            {
                Name = name,
                Path = path
            };

            if (_tableData.Any(x => x.Id == tableData.Id))
                return;

            tableData.Task = Task.Run(async () =>
            {
                var tableInfo = this._engine.ResolveTable(name, path);
                if (tableInfo == null)
                    throw new TSQLException($"Table or view does not exist: {name}");

                object results = null;
                List<(string Name, Type FieldType)> resultFields = null;

                if (tableInfo.MethodInfo != null)
                {
                    var method = tableInfo.MethodInfo;
                    var callFunction = Expression.Call(Expression.Constant(method.FunctionDelegate.Target), method.FunctionMethod);
                    var resultAsObjectExpression = Expression.Convert(callFunction, typeof(object));
                    results = Expression.Lambda<Func<object>>(resultAsObjectExpression).Compile()();
                }
                else
                {
                    results = tableInfo.Result;
                }

                if (results is Task)
                {
                    Task<object> resultsTask = (Task<object>)results;
                    await Task.WhenAll(resultsTask);
                    try
                    {
                        resultsTask.Wait();
                        results = resultsTask.Result;
                    } catch (AggregateException ex)
                    {
                        throw ex.InnerException;
                    }

                }

                var resultType = results.GetType();
                var resultItemsType = results.GetType().GetElementType();
                if (resultItemsType != null)
                {
                    resultFields = resultItemsType.GetProperties().Select(x => (x.Name, x.PropertyType)).ToList();
                }
                else if (typeof(IAsyncDataReader).IsAssignableFrom(resultType))
                {
                    var resultReader = (IAsyncDataReader)results;
                    resultItemsType = typeof(object[]);
                    resultFields = Enumerable
                        .Range(0, resultReader.FieldCount)
                        .Select(x => (resultReader.GetName(x), resultReader.GetFieldType(x)))
                        .ToList();
                    results = new AsyncDataReaderEnumerable(resultReader, ct);
                }
                else if (typeof(IDataReader).IsAssignableFrom(resultType))
                {
                    var resultReader = (IDataReader)results;
                    resultItemsType = typeof(object[]);
                    resultFields = Enumerable
                        .Range(0, resultReader.FieldCount)
                        .Select(x => (resultReader.GetName(x), resultReader.GetFieldType(x)))
                        .ToList();
                    results = new DataReaderEnumerable(resultReader, ct);
                }

                tableData.Results = results;
                tableData.ResultFields = resultFields;
                tableData.ResultType = resultType;
                tableData.ResultItemsType = resultItemsType;
            }, ct);
            _tableData.Add(tableData);
        }

        public async Task<TableResult> GeTable(string name, string[] path)
        {
            var tableData = new TableResult
            {
                Name = name,
                Path = path
            };

            var featchedData = _tableData.FirstOrDefault(x => x.Id == tableData.Id);
            var taskResult = await Task.WhenAny(featchedData.Task);
            
            try
            {
                taskResult.Wait();
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }

            return featchedData;
        }
    }

    public class TableResult
    {
        public string Name { get; set; }
        public string[] Path { get; set; }
        public object Results { get; set; }
        public List<(string Name, Type FieldType)> ResultFields { get; set; }
        public Type ResultType { get; set; }
        public Type ResultItemsType { get; set; }
        public string Id => $"{string.Join(".", this.Path)}.{this.Name}".ToLower();

        public Task Task { get; internal set; }
    }
}
