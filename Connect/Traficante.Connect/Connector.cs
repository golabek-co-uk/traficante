using System;
using System.Data;
using System.Threading;

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
    }

}
