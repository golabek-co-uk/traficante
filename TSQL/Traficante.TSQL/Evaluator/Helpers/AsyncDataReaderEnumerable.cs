using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Traficante.TSQL.Evaluator.Helpers
{
    public class AsyncDataReaderEnumerable : IEnumerable<Object[]>
    {
        private readonly CancellationToken _ct;

        public List<Object[]> Items { get; set; } = new List<object[]>();
        public Task Task { get; set; } = null;

        public AsyncDataReaderEnumerable(IAsyncDataReader reader, CancellationToken ct)
        {
            this._ct = ct;
            Task = Task.Run(async () =>
            {
                var nextResultTask = reader.NextResultAsync();
                while ((await Task.WhenAll(nextResultTask))[0] && nextResultTask.Result)
                {
                    nextResultTask = reader.NextResultAsync();
                    while (reader.Read())
                    {
                        Object[] row = new Object[reader.FieldCount];
                        reader.GetValues(row);
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            if (row[i] is DBNull)
                                row[i] = GetDefaultValue(reader.GetFieldType(i));
                        }
                        Items.Add(row);
                    }
                }
            }, this._ct);
        }

        public object GetDefaultValue(Type t)
        {
            if (t.IsValueType && Nullable.GetUnderlyingType(t) == null)
            {
                return Activator.CreateInstance(t);
            }
            else
            {
                return null;
            }
        }

        public IEnumerator<object[]> GetEnumerator()
        {
            return new AsyncDataReaderEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new AsyncDataReaderEnumerator(this);
        }
    }

    public class AsyncDataReaderEnumerator : IEnumerator<object[]>
    {
        private AsyncDataReaderEnumerable _dataReader = null;
        private int _currentIndex = -1;

        public AsyncDataReaderEnumerator(AsyncDataReaderEnumerable enumerable)
        {
            this._dataReader = enumerable;
        }

        public object[] Current => _dataReader.Items[_currentIndex];

        object IEnumerator.Current => _dataReader.Items[_currentIndex];

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            if (_dataReader.Items.Count > this._currentIndex + 1)
            {
                this._currentIndex = this._currentIndex + 1;
                return true;
            }
            if (_dataReader.Task.IsCompleted == false)
            {
                Thread.Sleep(100);
                return MoveNext();
            }
            if (_dataReader.Items.Count > this._currentIndex + 1)
            {
                this._currentIndex = this._currentIndex + 1;
                return true;
            }
            return false;
        }

        public void Reset()
        {
            this._currentIndex = -1;
        }
    }
}
