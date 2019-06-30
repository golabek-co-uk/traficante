using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Musoq.Schema;
using Musoq.Schema.DataSources;
using Musoq.Schema.Reflection;

namespace Musoq.Evaluator.Tests.Core.Schema
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