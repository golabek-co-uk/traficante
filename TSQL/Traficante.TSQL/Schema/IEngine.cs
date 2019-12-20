using System;
using System.Collections;
using System.Reflection;
using Traficante.TSQL.Schema.DataSources;

namespace Traficante.TSQL.Schema
{
    public interface IEngine
    {
        Variable GetVariable(string name);
        void SetVariable<T>(string name, T value);
        void SetVariable(string name, Type type, object value);

        Traficante.TSQL.Schema.Managers.MethodInfo ResolveMethod(string[] path, string method, Type[] parameters);

        TableInfo ResolveTable(string name, string[] path);
    }
}