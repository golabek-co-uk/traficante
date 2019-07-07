using System;
using System.Collections.Generic;
using System.Text;
using Traficante.TSQL.Lib;
using Traficante.TSQL.Schema.DataSources;
using Traficante.TSQL.Schema.Managers;

namespace Traficante.TSQL.Schema
{
    public class ServerSchema : BaseSchema
    {
        public ServerSchema(): base("schema", CreateLibrary())
        {
        }

        private static MethodsAggregator CreateLibrary()
        {
            var methodManager = new MethodsManager();
            var propertiesManager = new PropertiesManager();

            var lib = new Library();

            propertiesManager.RegisterProperties(lib);
            methodManager.RegisterLibraries(lib);

            return new MethodsAggregator(methodManager, propertiesManager);
        }
    }
}
