using System;
using System.Collections.Generic;
using System.Text;
using Traficante.TSQL.Lib;
using Traficante.TSQL.Plugins;
using Traficante.TSQL.Schema.DataSources;
using Traficante.TSQL.Schema.Managers;

namespace Traficante.TSQL.Schema
{
    public class Database : BaseDatabase
    {
        public Database(string database, string defaultSchema): base(database, defaultSchema, CreateLibrary(null))
        {
        }

        public Database(string database, string defaultSchema, LibraryBase library) : base(database, defaultSchema, CreateLibrary(library))
        {
        }

        private static MethodsAggregator CreateLibrary(LibraryBase library)
        {
            var methodManager = new MethodsManager();
            //var propertiesManager = new PropertiesManager();

            if (library == null)
                library = new Library();

            //propertiesManager.RegisterProperties(library);
            methodManager.RegisterLibraries(library);

            return new MethodsAggregator(methodManager);//, propertiesManager);
        }
    }
}
