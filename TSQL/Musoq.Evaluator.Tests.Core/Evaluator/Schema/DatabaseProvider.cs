using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Traficante.TSQL.Schema;
using Traficante.TSQL.Schema.DataSources;
using Traficante.TSQL.Schema.Reflection;

namespace Traficante.TSQL.Evaluator.Tests.Core.Schema
{
    public class DatabaseProvider : IDatabaseProvider
    {
        private IEnumerable<IDatabase> _databases;

        public DatabaseProvider(IEnumerable<IDatabase> databases)
        {
            this._databases = databases;
        }

        public IDatabase GetDatabase(string database)
        {
            if (database != null)
                return _databases.FirstOrDefault(x => x.Name == database);
            return _databases.FirstOrDefault();
        }
    }
}