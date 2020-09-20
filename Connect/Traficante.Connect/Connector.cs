using System;
using System.Collections;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Traficante.Connect
{
    public class Connector
    {
        public ConnectorConfig Config { get; set; }

        public virtual Delegate ResolveMethod(string[] path, Type[] arguments, CancellationToken ct)
        {
            return null;
        }

        public virtual Delegate ResolveTable(string[] path, CancellationToken ct)
        {
            return null;
        }

        public virtual Task<object> RunQuery(string query, string language, string[] path, CancellationToken ct)
        {
            return null;
        }
    }

}
