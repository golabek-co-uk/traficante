using System;
using System.Collections;
using System.Reflection;

namespace Traficante.TSQL.Schema
{
    public interface IEngine
    {
        IVariable GetVariable(string name);
        void SetVariable<T>(string name, T value);
        void SetVariable<T>(string database, string schema, string name, T value);
        void SetVariable(string name, Type type, object value);
        void SetVariable(string database, string schema, string name, Type type, object value);

        MethodInfo ResolveMethod(string[] path, string method, Type[] parameters);

        TableDef GetTable(string name, string[] path);
        TableValuedFunctionDef GetTableValuedFunction(string name, string[] path);
    }
}