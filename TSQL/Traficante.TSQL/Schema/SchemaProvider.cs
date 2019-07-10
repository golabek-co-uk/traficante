using System.Collections.Generic;
using System.Linq;

namespace Traficante.TSQL.Schema
{
    public class DatabaseProvider : IDatabaseProvider
    {
        private IEnumerable<IDatabase> _databases;
        private string _defaultDatabase;

        public DatabaseProvider(IEnumerable<IDatabase> databases, string defaultDatabase)
        {
            this._databases = databases;
            this._defaultDatabase = defaultDatabase;
        }

        public IDatabase GetDatabase(string database)
        {
            if (database != null)
                return _databases.FirstOrDefault(x => string.Equals(x.Name, database, System.StringComparison.CurrentCultureIgnoreCase));
            else
                return _databases.FirstOrDefault(x => string.Equals(x.Name, _defaultDatabase));
        }
    }
}