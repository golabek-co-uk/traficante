using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using Traficante.Studio.Models;
using Traficante.Studio.Services;

namespace Traficante.Studio
{
    public static class Interactions
    {
        public static readonly Interaction<SqlServerConnectionInfo, SqlServerConnectionInfo> ConnectToSqlServer = new Interaction<SqlServerConnectionInfo, SqlServerConnectionInfo>();
        public static readonly Interaction<Unit, Unit> NewQuery = new Interaction<Unit, Unit>();
        public static readonly Interaction<Exception, Unit> Exceptions = new Interaction<Exception, Unit>();

    }
}
