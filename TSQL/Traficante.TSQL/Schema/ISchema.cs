using System;
using System.Reflection;
using Traficante.TSQL.Schema.DataSources;

namespace Traficante.TSQL.Schema
{
    public interface IDatabase
    {
        string Name { get; }

        ITable GetTableByName(string schema, string name);

        RowSource GetTableRowSource(string schema, string name);

        ITable GetFunctionByName(string schema, string name, object[] parameters);

        RowSource GetFunctionRowSource(string schema, string name, object[] parameters);


        //Reflection.SchemaMethodInfo[] GetConstructors(string schema, string methodName);

        //Reflection.SchemaMethodInfo[] GetConstructors(string schema);

        //Reflection.SchemaMethodInfo[] GetRawConstructors(string schema);

        //Reflection.SchemaMethodInfo[] GetRawConstructors(string schema, string methodName);

        //RowSource GetRowSource(string schema, string name, RuntimeContext interCommunicator, params object[] parameters);

        MethodInfo ResolveMethod(string schema, string method, Type[] parameters);

        bool TryResolveAggreationMethod(string method, Type[] parameters, out MethodInfo methodInfo);
        
    }
}