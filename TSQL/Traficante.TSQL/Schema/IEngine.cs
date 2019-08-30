using System;

namespace Traficante.TSQL.Schema
{
    public interface IEngine
    {
        IDatabase GetDatabase(string database);
        IVariable GetVariable(string name);
        void SetVariable<T>(string name, T value);
        void SetVariable<T>(string database, string schema, string name, T value);
        void SetVariable(string name, Type type, object value);
        void SetVariable(string database, string schema, string name, Type type, object value);
    }
}