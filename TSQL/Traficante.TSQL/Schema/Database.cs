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
        public Database(string database, string defaultSchema) : base(database, defaultSchema)
        {
        }
    }
}
