using System;
using System.Data;
using System.Threading;

namespace Traficante.Connect
{
    public class Connector
    {
        public ConnectorConfig Config { get; set; }

        public virtual Delegate ResolveMethod(string name, string[] path, Type[] arguments, CancellationToken ct)
        {
            return null;
        }

        public virtual Delegate ResolveTable(string name, string[] path, CancellationToken ct)
        {
            return null;
        }
    }

}
