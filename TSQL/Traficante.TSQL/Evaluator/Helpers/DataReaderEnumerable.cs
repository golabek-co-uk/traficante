using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Traficante.TSQL.Evaluator.Helpers
{
    public class DataReaderEnumerable : IEnumerable<Object[]>
    {
        private readonly CancellationToken _cancellationToken;

        public List<Object[]> List { get; set; } = new List<object[]>();
        public Task Task { get; set; } = null;

        public DataReaderEnumerable(IDataReader source, CancellationToken cancellationToken)
        {
            this._cancellationToken = cancellationToken;
            Task = Task.Run(() =>
            {
                try
                {
                    while (source.Read())
                    {
                        Object[] row = new Object[source.FieldCount];
                        source.GetValues(row);
                        for (int i = 0; i < source.FieldCount; i++)
                        {
                            if (row[i] is DBNull)
                                row[i] = GetDefaultValue(source.GetFieldType(i));
                        }
                        //yield return row;
                        List.Add(row);
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }
                finally
                {
                    source.Dispose();
                }
            }, cancellationToken);
           
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
            return new DataReaderEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new DataReaderEnumerator(this);
        }
    }

    public class DataReaderEnumerator : IEnumerator<object[]>
    {
        private DataReaderEnumerable _dataReader = null;
        private int _currentIndex = -1;

        public DataReaderEnumerator(DataReaderEnumerable enumerable)
        {
            this._dataReader = enumerable;
        }

        public object[] Current => _dataReader.List[_currentIndex];

        object IEnumerator.Current => _dataReader.List[_currentIndex];

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            if (_dataReader.List.Count > this._currentIndex + 1)
            {
                this._currentIndex = this._currentIndex + 1;
                return true;
            }
            if (_dataReader.Task.IsCompleted == false)
            {
                Thread.Sleep(100);
                return MoveNext();
            }
            if (_dataReader.List.Count > this._currentIndex + 1)
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
