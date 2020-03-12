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
        public static readonly Interaction<SqlServerObjectModel, SqlServerObjectModel> ConnectToSqlServer = new Interaction<SqlServerObjectModel, SqlServerObjectModel>();
        public static readonly Interaction<MySqlObjectModel, MySqlObjectModel> ConnectToMySql = new Interaction<MySqlObjectModel, MySqlObjectModel>();
        public static readonly Interaction<SqliteObjectModel, SqliteObjectModel> ConnectToSqlite = new Interaction<SqliteObjectModel, SqliteObjectModel>();
        
        public static readonly Interaction<Unit, Unit> NewQuery = new Interaction<Unit, Unit>();
        public static readonly Interaction<Exception, Unit> Exceptions = new Interaction<Exception, Unit>();

    }
}
