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
        public static readonly Interaction<SqlServerConnectionModel, SqlServerConnectionModel> ConnectToSqlServer = new Interaction<SqlServerConnectionModel, SqlServerConnectionModel>();
        public static readonly Interaction<MySqlConnectionModel, MySqlConnectionModel> ConnectToMySql = new Interaction<MySqlConnectionModel, MySqlConnectionModel>();
        public static readonly Interaction<Unit, Unit> NewQuery = new Interaction<Unit, Unit>();
        public static readonly Interaction<Exception, Unit> Exceptions = new Interaction<Exception, Unit>();

    }
}
